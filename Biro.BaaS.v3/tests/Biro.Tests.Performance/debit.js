import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m30s', target: 100 },
    { duration: '20s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<100'], // 95% of requests must complete below 100ms
    http_reqs: ['rate>1000'], // must sustain 1000 reqs/s
  },
};

export default function () {
  const url = 'http://localhost:5002/debit';
  const payload = JSON.stringify({
    accountId: 'f4b3b3b4-4b3b-4b3b-8b3b-4b3b3b4b3b3b',
    amount: 10,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(url, payload, params);

  check(res, {
    'is status 200': (r) => r.status === 200,
  });

  sleep(1);
}
