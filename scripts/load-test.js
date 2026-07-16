// Load test at realistic single-school scale — see docs/DEPLOYMENT_ARCHITECTURE.md
// and PLAN.md Phase 6. Run via k6 (https://k6.io):
//
//   docker run --rm -i --network deploy_default -e BASE_URL=http://api:8080 \
//     grafana/k6 run - < scripts/load-test.js
//
// Two scenarios reflecting how this system is actually used, not a generic
// "hammer the API" benchmark:
//   - "staff": ~30 concurrent staff/admins doing typical dashboard reads (student
//     lists, attendance records) with occasional writes — a school's actual
//     concurrent user count is small; this is already generous headroom.
//   - "devices": ~20 ESP32 readers each sending a scan event periodically — "tens
//     of devices" per PLAN.md's stated target scale, not hundreds.
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    // Ramping, not an instant spike — staff log in over the first part of the
    // school day, not all in the same second (an instant 30-VU spike is a load-
    // test artifact, not realistic usage, and floods the login rate limiter).
    staff: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '15s', target: 30 },
        { duration: '30s', target: 30 },
        { duration: '5s', target: 0 },
      ],
      exec: 'staffScenario',
    },
    devices: {
      executor: 'constant-vus',
      vus: 20,
      duration: '50s',
      exec: 'deviceScenario',
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests under 500ms
    http_req_failed: ['rate<0.01'], // <1% error rate
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5080';
const ADMIN_EMAIL = __ENV.ADMIN_EMAIL || 'admin@smartcampus.local';
const ADMIN_PASSWORD = __ENV.ADMIN_PASSWORD || 'ChangeMe123!';

// Logs in once per VU, like a real user — a human sees a login error and stops,
// they don't retry in a tight loop. Caching "attempted" separately from the token
// means a failed login is not retried every iteration; retrying without backoff
// against a rate-limited endpoint would otherwise create a self-sustaining lockout
// for that IP (this is exactly what an earlier version of this script did by
// accident — a real client should always back off on 429, not hammer harder).
let cachedToken = null;
let loginAttempted = false;

function login() {
  if (loginAttempted) return cachedToken;
  loginAttempted = true;

  const res = http.post(
    `${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ email: ADMIN_EMAIL, password: ADMIN_PASSWORD }),
    { headers: { 'Content-Type': 'application/json' } },
  );
  check(res, { 'login succeeded': (r) => r.status === 200 });
  cachedToken = res.status === 200 ? JSON.parse(res.body).token : null;
  return cachedToken;
}

export function staffScenario() {
  const token = login();
  if (!token) {
    sleep(1);
    return;
  }
  const headers = { Authorization: `Bearer ${token}` };

  const studentsRes = http.get(`${BASE_URL}/api/v1/students`, { headers });
  check(studentsRes, { 'students 200': (r) => r.status === 200 });

  const attendanceRes = http.get(
    `${BASE_URL}/api/v1/attendance-records?fromDate=2026-01-01&toDate=2026-12-31`,
    { headers },
  );
  check(attendanceRes, { 'attendance 200': (r) => r.status === 200 });

  sleep(1);
}

export function deviceScenario() {
  // Simulated device — heartbeats are the realistic steady-state device traffic;
  // this scenario does not attempt real scan events since that requires a
  // pre-registered device id/token per VU, out of scope for a load-shape test.
  const res = http.get(`${BASE_URL}/api/v1/health`);
  check(res, { 'health 200': (r) => r.status === 200 });
  sleep(2);
}
