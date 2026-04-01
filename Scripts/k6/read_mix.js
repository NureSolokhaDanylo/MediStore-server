import http from 'k6/http';
import { check } from 'k6';

const baseUrl = (String(__ENV.BASE_URL || 'https://api.medistore.app')).replace(/\/+$/, '');
const defaultJwtToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjBiOGZmZjkzLTQ0OTAtNDViZS1hZTcxLTZjYmNkYzc0M2MyMSIsImp0aSI6IjkwOGQ5ZTNiLTMwZDUtNDEwMS1hZGI3LWQ1M2U5NmQ2YjhkOSIsImlhdCI6MTc3NTA0OTI2MSwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjpbIk9ic2VydmVyIiwiQWRtaW4iXSwiYXVkIjoiTWVkaVN0b3JlU2VydmVyIiwiaXNzIjoiTWVkaVN0b3JlU2VydmVyIiwiZXhwIjoxNzc1MTA5MjYxLCJuYmYiOjE3NzUwNDkyNjF9.l11sJg18LtUFL7pd6DoqJXD42ActsbKZazZCU9gw2jY';
const jwtToken = String(__ENV.JWT_TOKEN || defaultJwtToken);

if (!jwtToken) {
  throw new Error('JWT_TOKEN env var is required');
}

const rate = Number(__ENV.READ_RATE || 80);
const duration = String(__ENV.READ_DURATION || '2m');
const preAllocatedVUs = Number(__ENV.READ_PREALLOCATED_VUS || Math.max(30, Math.ceil(rate * 1.5)));
const maxVUs = Number(__ENV.READ_MAX_VUS || Math.max(100, Math.ceil(rate * 4)));

export const options = {
  scenarios: {
    read_mix: {
      executor: 'constant-arrival-rate',
      rate,
      timeUnit: '1s',
      duration,
      preAllocatedVUs,
      maxVUs,
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<1200', 'p(99)<2500'],
    checks: ['rate>0.99'],
  },
};

function authHeaders() {
  return {
    headers: {
      Authorization: `Bearer ${jwtToken}`,
    },
  };
}

export default function () {
  const useAlerts = Math.random() < 0.7;
  let res;

  if (useAlerts) {
    const url = `${baseUrl}/api/v1/alerts/filtered?Skip=0&Take=50`;
    res = http.get(url, { ...authHeaders(), tags: { endpoint: 'alerts_filtered' } });
  } else {
    const url = `${baseUrl}/api/v1/sensors/paged?skip=0&take=50&q=TEMP&sensorType=Temperature`;
    res = http.get(url, { ...authHeaders(), tags: { endpoint: 'sensors_paged' } });
  }

  check(res, {
    'status is 200': (r) => r.status === 200,
  });
}
