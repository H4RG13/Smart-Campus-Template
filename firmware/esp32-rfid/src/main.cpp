// SmartCampus RFID reader firmware — ESP32 + MFRC522
//
// Responsibilities here are deliberately narrow (see docs/IOT_COMMUNICATION.md and
// RULES.md #4): read the tag, authenticate, queue/retry offline, heartbeat, and give
// LED/buzzer feedback based on the server's response. Every decision about whether a
// scan counts as on-time, late, or a duplicate is made server-side — this firmware
// never computes that itself.
//
// NOTE: this is reference/unverified code, written to match the documented protocol
// but not flashed to real hardware or bench-tested in this environment. Treat it as a
// starting point to validate against actual ESP32 + MFRC522 hardware, not as
// production-ready firmware.

#include <Arduino.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <SPI.h>
#include <MFRC522.h>
#include <ArduinoJson.h>
#include <LittleFS.h>
#include "Config.h"

MFRC522 rfid(RFID_SS_PIN, RFID_RST_PIN);

static const char *QUEUE_FILE = "/queue.jsonl";
static unsigned long lastHeartbeatAtMs = 0;
static unsigned long lastQueueFlushAtMs = 0;
static uint32_t localEventSequence = 0;

// ---------------------------------------------------------------------------
// Feedback (LED/buzzer) — purely presentational, driven by the server's response
// ---------------------------------------------------------------------------

void feedbackSetup()
{
    pinMode(LED_GREEN_PIN, OUTPUT);
    pinMode(LED_RED_PIN, OUTPUT);
    pinMode(BUZZER_PIN, OUTPUT);
}

void feedbackAccepted(const String &attendanceStatus)
{
    // OnTime -> steady green + short beep; Late -> green blink + short beep.
    digitalWrite(LED_GREEN_PIN, HIGH);
    digitalWrite(BUZZER_PIN, HIGH);
    delay(120);
    digitalWrite(BUZZER_PIN, LOW);
    if (attendanceStatus == "Late")
    {
        delay(150);
        digitalWrite(LED_GREEN_PIN, LOW);
        delay(150);
        digitalWrite(LED_GREEN_PIN, HIGH);
    }
    delay(400);
    digitalWrite(LED_GREEN_PIN, LOW);
}

void feedbackRejected()
{
    digitalWrite(LED_RED_PIN, HIGH);
    digitalWrite(BUZZER_PIN, HIGH);
    delay(400);
    digitalWrite(BUZZER_PIN, LOW);
    digitalWrite(LED_RED_PIN, LOW);
}

void feedbackOfflineQueued()
{
    // Distinct short double-blink so staff can tell "queued, not yet confirmed"
    // apart from a real accept/reject during a network outage.
    for (int i = 0; i < 2; i++)
    {
        digitalWrite(LED_GREEN_PIN, HIGH);
        delay(80);
        digitalWrite(LED_GREEN_PIN, LOW);
        delay(80);
    }
}

// ---------------------------------------------------------------------------
// WiFi
// ---------------------------------------------------------------------------

void wifiConnect()
{
    if (WiFi.status() == WL_CONNECTED)
    {
        return;
    }

    WiFi.mode(WIFI_STA);
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

    Serial.print("Connecting to WiFi");
    unsigned long start = millis();
    while (WiFi.status() != WL_CONNECTED && millis() - start < 15000)
    {
        delay(250);
        Serial.print(".");
    }
    Serial.println();

    if (WiFi.status() == WL_CONNECTED)
    {
        Serial.printf("WiFi connected, IP=%s\n", WiFi.localIP().toString().c_str());
    }
    else
    {
        Serial.println("WiFi connect failed — will retry in main loop.");
    }
}

// ---------------------------------------------------------------------------
// Offline queue — one JSON object per line in a LittleFS file (durable across reboots)
// ---------------------------------------------------------------------------

int queueDepth()
{
    File file = LittleFS.open(QUEUE_FILE, "r");
    if (!file)
    {
        return 0;
    }

    int count = 0;
    while (file.available())
    {
        if (file.readStringUntil('\n').length() > 0)
        {
            count++;
        }
    }
    file.close();
    return count;
}

void queueAppend(const String &jsonLine)
{
    if (queueDepth() >= MAX_QUEUED_EVENTS)
    {
        Serial.println("Offline queue full — dropping oldest event is not implemented; discarding new event.");
        return;
    }

    File file = LittleFS.open(QUEUE_FILE, "a");
    if (!file)
    {
        Serial.println("Failed to open queue file for append.");
        return;
    }
    file.println(jsonLine);
    file.close();
}

// Removes the first line from the queue file (the event that was just confirmed
// sent) by rewriting to a temp file — simple and correct at this queue's expected
// scale (tens of pending events during a network outage, not thousands).
void queuePopFront()
{
    File src = LittleFS.open(QUEUE_FILE, "r");
    if (!src)
    {
        return;
    }

    File dst = LittleFS.open("/queue.tmp", "w");
    bool skippedFirst = false;
    while (src.available())
    {
        String line = src.readStringUntil('\n');
        if (line.length() == 0)
        {
            continue;
        }
        if (!skippedFirst)
        {
            skippedFirst = true;
            continue;
        }
        dst.println(line);
    }
    src.close();
    dst.close();

    LittleFS.remove(QUEUE_FILE);
    LittleFS.rename("/queue.tmp", QUEUE_FILE);
}

bool queuePeekFront(String &outLine)
{
    File file = LittleFS.open(QUEUE_FILE, "r");
    if (!file)
    {
        return false;
    }

    while (file.available())
    {
        String line = file.readStringUntil('\n');
        if (line.length() > 0)
        {
            outLine = line;
            file.close();
            return true;
        }
    }
    file.close();
    return false;
}

// ---------------------------------------------------------------------------
// API calls
// ---------------------------------------------------------------------------

// Returns true if the server responded at all (even with a rejection — a scan event
// is a fact that was received, per docs/API_SPECIFICATION.md, not a client error).
// Returns false only on a network-level failure, which is what triggers queuing.
bool submitEvent(const String &deviceEventId, const String &rfidTagId, time_t deviceTimestampUtc, String &responseBody)
{
    if (WiFi.status() != WL_CONNECTED)
    {
        return false;
    }

    HTTPClient http;
    http.begin(String(API_BASE_URL) + "/devices/" + DEVICE_ID + "/events");
    http.addHeader("Content-Type", "application/json");
    http.addHeader("Authorization", String("Bearer ") + DEVICE_AUTH_TOKEN);
    http.setTimeout(8000);

    JsonDocument doc;
    doc["deviceEventId"] = deviceEventId;
    doc["rfidTagId"] = rfidTagId;
    char isoTimestamp[25];
    strftime(isoTimestamp, sizeof(isoTimestamp), "%Y-%m-%dT%H:%M:%SZ", gmtime(&deviceTimestampUtc));
    doc["deviceTimestampUtc"] = isoTimestamp;

    String body;
    serializeJson(doc, body);

    int statusCode = http.POST(body);
    if (statusCode > 0)
    {
        responseBody = http.getString();
    }
    http.end();

    return statusCode > 0;
}

void submitHeartbeat()
{
    if (WiFi.status() != WL_CONNECTED)
    {
        return;
    }

    HTTPClient http;
    http.begin(String(API_BASE_URL) + "/devices/" + DEVICE_ID + "/heartbeat");
    http.addHeader("Content-Type", "application/json");
    http.addHeader("Authorization", String("Bearer ") + DEVICE_AUTH_TOKEN);
    http.setTimeout(8000);

    JsonDocument doc;
    doc["firmwareVersion"] = FIRMWARE_VERSION;
    doc["signalStrength"] = WiFi.RSSI();
    doc["queueDepth"] = queueDepth();

    String body;
    serializeJson(doc, body);

    int statusCode = http.POST(body);
    if (statusCode <= 0)
    {
        Serial.println("Heartbeat failed to send (offline) — will retry on next interval.");
    }
    http.end();
}

// Drains the offline queue front-to-back, stopping at the first network failure so
// events stay in order and nothing is lost if connectivity drops mid-flush.
void flushQueue()
{
    String line;
    while (queuePeekFront(line))
    {
        JsonDocument doc;
        if (deserializeJson(doc, line) != DeserializationError::Ok)
        {
            queuePopFront(); // corrupt line — drop it rather than block the queue forever
            continue;
        }

        String responseBody;
        bool sent = submitEvent(doc["deviceEventId"], doc["rfidTagId"], (time_t)doc["deviceTimestampUtc"].as<long>(), responseBody);
        if (!sent)
        {
            break; // still offline — try again next interval
        }

        queuePopFront();
    }
}

// ---------------------------------------------------------------------------
// RFID
// ---------------------------------------------------------------------------

String readTagUidHex()
{
    String uid;
    for (byte i = 0; i < rfid.uid.size; i++)
    {
        if (rfid.uid.uidByte[i] < 0x10)
        {
            uid += "0";
        }
        uid += String(rfid.uid.uidByte[i], HEX);
    }
    uid.toUpperCase();
    return uid;
}

void handleScan(const String &tagUid)
{
    time_t now;
    time(&now);

    // deviceId is embedded so a duplicate deviceEventId is only ever compared within
    // this device's own event history — see docs/API_SPECIFICATION.md.
    String deviceEventId = String(DEVICE_ID).substring(0, 8) + "-" + String(localEventSequence++);

    JsonDocument doc;
    doc["deviceEventId"] = deviceEventId;
    doc["rfidTagId"] = tagUid;
    doc["deviceTimestampUtc"] = (long)now;
    String line;
    serializeJson(doc, line);

    // Always queue first — this is what makes the offline path safe: if the
    // immediate send below fails, the event is already durably recorded.
    queueAppend(line);

    String responseBody;
    if (submitEvent(deviceEventId, tagUid, now, responseBody))
    {
        queuePopFront();

        JsonDocument response;
        if (deserializeJson(response, responseBody) == DeserializationError::Ok)
        {
            const char *processingStatus = response["processingStatus"] | "Accepted";
            if (String(processingStatus) == "Accepted")
            {
                feedbackAccepted(response["attendanceStatus"] | "");
            }
            else
            {
                feedbackRejected();
            }
        }
    }
    else
    {
        feedbackOfflineQueued();
    }
}

// ---------------------------------------------------------------------------
// Arduino entry points
// ---------------------------------------------------------------------------

void setup()
{
    Serial.begin(115200);
    feedbackSetup();

    if (!LittleFS.begin(true))
    {
        Serial.println("LittleFS mount failed — offline queue will not persist across reboots.");
    }

    SPI.begin();
    rfid.PCD_Init();

    wifiConnect();

    Serial.printf("SmartCampus RFID reader — firmware %s, device %s\n", FIRMWARE_VERSION, DEVICE_ID);
}

void loop()
{
    wifiConnect(); // no-op if already connected; retries on drop

    if (millis() - lastQueueFlushAtMs > QUEUE_FLUSH_INTERVAL_MS)
    {
        flushQueue();
        lastQueueFlushAtMs = millis();
    }

    if (millis() - lastHeartbeatAtMs > HEARTBEAT_INTERVAL_MS)
    {
        submitHeartbeat();
        lastHeartbeatAtMs = millis();
    }

    if (rfid.PICC_IsNewCardPresent() && rfid.PICC_ReadCardSerial())
    {
        String tagUid = readTagUidHex();
        Serial.printf("Scanned tag: %s\n", tagUid.c_str());
        handleScan(tagUid);
        rfid.PICC_HaltA();
    }

    delay(50);
}
