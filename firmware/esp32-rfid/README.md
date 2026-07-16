# SmartCampus RFID Reader Firmware

ESP32 + MFRC522 firmware implementing the device side of `docs/IOT_COMMUNICATION.md`.

> **Status: reference implementation, not bench-tested.** This code was written to
> match the documented protocol (device auth, idempotent event submission, offline
> queue + retry, heartbeat, LED/buzzer feedback) but has not been flashed to real
> hardware or run against actual ESP32 + MFRC522 boards. Validate it against real
> hardware before relying on it for a deployment, and expect to iterate — timing,
> pin assignments, and library API details in particular are the kind of thing that
> only surface once real hardware is in the loop.

## Hardware

- ESP32 dev board
- MFRC522 RFID reader (SPI)
- 2 LEDs (green = accepted, red = rejected) + a buzzer, or a single RGB status LED
- Wiring: see pin definitions in `include/Config.h.example`

## Setup

1. Install [PlatformIO](https://platformio.org/) (VS Code extension or CLI).
2. Copy `include/Config.h.example` to `include/Config.h` and fill in:
   - WiFi SSID/password
   - The school's API base URL
   - The device id and auth token from `POST /devices/register` (shown once —
     see `docs/API_SPECIFICATION.md`)
3. `pio run --target upload` to build and flash.
4. `pio device monitor` to watch serial output.

`Config.h` is gitignored — never commit real WiFi credentials or a device token.

## Behavior

- Reads an RFID tag, generates a locally unique `deviceEventId`, and always writes it
  to the LittleFS-backed offline queue **before** attempting to send — this is what
  makes the offline path safe (see `handleScan` in `src/main.cpp`).
- Submits the event to `POST /devices/{deviceId}/events`. On success (any HTTP
  response, including a rejection), the event is popped off the queue. On a network
  failure, it stays queued and is retried by the queue-flush loop.
- Sends a heartbeat (`firmwareVersion`, `signalStrength`, `queueDepth`) every
  `HEARTBEAT_INTERVAL_MS` — keep this in sync with the backend's
  `School:Iot:HeartbeatIntervalSeconds` so the offline-detection threshold on the
  dashboard makes sense.
- LED/buzzer feedback is driven entirely by the server's response — this firmware
  never decides on-time/late/rejected itself (per `RULES.md` #4).

## Known gaps / next steps for whoever picks this up

- No NTP time sync is wired in — `time(&now)` assumes the ESP32's RTC has already
  been set. Add a `configTime()` call in `setup()` against an NTP server before
  relying on `deviceTimestampUtc` being accurate.
- No debounce beyond `PICC_HaltA()` — rapid repeated taps of the same card may need
  an explicit cooldown window if that turns out to be an issue in practice.
- HTTPS/TLS is not configured (`HTTPClient` here talks plain HTTP) — production
  deployments terminate TLS at the school's NGINX (see
  `docs/DEPLOYMENT_ARCHITECTURE.md`), so the ESP32 will need a root CA cert loaded
  via `WiFiClientSecure` to call the real HTTPS endpoint.
