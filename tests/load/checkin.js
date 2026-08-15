/**
 * Teste de carga — POST /checkin
 *
 * Objetivo: validar que o endpoint de check-in suporta carga básica com p95 < 500ms.
 * O token JWT é obtido uma única vez no setup() e compartilhado entre os VUs.
 * Cada iteração usa um idLocal único (UUID v4) para simular inserção real (204).
 *
 * Uso:
 *   k6 run tests/load/checkin.js
 *   k6 run tests/load/checkin.js -e BASE_URL=https://provida-api.enzojb.com.br
 *   k6 run tests/load/checkin.js -e EMAIL=usuario@teste.com -e SENHA=Senha@123
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

const BASE_URL = __ENV.BASE_URL || 'https://provida-api.enzojb.com.br';
const EMAIL    = __ENV.EMAIL   || 'teste@provavida.com';
const SENHA    = __ENV.SENHA   || 'Senha@123';

// Métricas customizadas
const checkinDuration    = new Trend('checkin_duration', true);
const checkinSuccessRate = new Rate('checkin_success_rate');

export const options = {
  stages: [
    { duration: '30s', target: 10 }, // rampa de subida
    { duration: '2m',  target: 50 }, // carga sustentada
    { duration: '30s', target: 0  }, // rampa de descida
  ],
  thresholds: {
    // RNF: p95 abaixo de 500ms
    'checkin_duration': ['p(95)<500'],
    'checkin_success_rate': ['rate>0.95'],
    'http_req_failed': ['rate<0.05'],
  },
};

// Setup: obtém token JWT uma vez para todos os VUs
export function setup() {
  const payload = JSON.stringify({ email: EMAIL, senha: SENHA });
  const params  = { headers: { 'Content-Type': 'application/json' } };

  const res = http.post(`${BASE_URL}/auth/login`, payload, params);

  if (res.status !== 200) {
    throw new Error(`Login falhou no setup: status ${res.status} — ${res.body}`);
  }

  const token = res.json('token');
  if (!token) {
    throw new Error('Token JWT nao retornado no setup');
  }

  console.log('Setup: login bem-sucedido, token obtido.');
  return { token };
}

export default function (data) {
  const params = {
    headers: {
      'Content-Type':  'application/json',
      'Authorization': `Bearer ${data.token}`,
    },
  };

  const payload = JSON.stringify({
    idLocal:   uuidv4(),                    // UUID único por iteração → sempre 204
    dataHora:  new Date().toISOString(),
    latitude:  -23.5505,
    longitude: -46.6333,
    deviceId:  'k6-load-test',
  });

  const res = http.post(`${BASE_URL}/checkin`, payload, params);

  const ok = check(res, {
    'checkin aceito': (r) => r.status === 204 || r.status === 200,
  });

  checkinDuration.add(res.timings.duration);
  checkinSuccessRate.add(ok);

  sleep(0.5);
}
