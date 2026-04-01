import http from 'k6/http';
import { check } from 'k6';

const baseUrl = (String(__ENV.BASE_URL || 'https://api.medistore.app')).replace(/\/+$/, '');
const defaultSensorApiKey = 'i1WAZyvXjJSeon1hQLFvP+k8GPBN0ZEZIQsF3JITrKc=';
const sensorApiKey = String(__ENV.SENSOR_API_KEY || defaultSensorApiKey);

if (!sensorApiKey) {
  throw new Error('SENSOR_API_KEY env var is required');
}

const rate = Number(__ENV.WRITE_RATE || 10);
const duration = String(__ENV.WRITE_DURATION || '2m');
const preAllocatedVUs = Number(__ENV.WRITE_PREALLOCATED_VUS || Math.max(10, Math.ceil(rate * 1.5)));
const maxVUs = Number(__ENV.WRITE_MAX_VUS || Math.max(40, Math.ceil(rate * 4)));

export const options = {
  scenarios: {
    write_readings: {
      executor: 'constant-arrival-rate',
      rate,
      timeUnit: '1s',
      duration,
      preAllocatedVUs,
      maxVUs,
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<1500', 'p(99)<3000'],
    checks: ['rate>0.98'],
  },
};

function buildPayload() {
  const min = 1.0;
  const max = 30.0;
  const value = min + Math.random() * (max - min);

  return JSON.stringify({
    timeStamp: new Date().toISOString(),
    value: Number(value.toFixed(2)),
  });
}

export default function () {
  const url = `${baseUrl}/api/v1/readings`;
  const res = http.post(url, buildPayload(), {
    headers: {
      'Content-Type': 'application/json',
      'X-Sensor-Api-Key': sensorApiKey,
    },
    tags: { endpoint: 'readings_create' },
  });

  check(res, {
    'status is 200 or 204': (r) => r.status === 200 || r.status === 204,
  });
}
