/**
 * Teste de carga — POST /auth/login
 *
 * Objetivo: validar que o endpoint de login suporta carga básica.
 * Nota: BCrypt é intencionalmente lento — threshold de p95 relaxado para 2s.
 *
 * Uso:
 *   k6 run tests/load/login.js
 *   k6 run tests/load/login.js -e BASE_URL=https://provida-api.enzojb.com.br
 *   k6 run tests/load/login.js -e EMAIL=usuario@teste.com -e SENHA=Senha@123
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'https://provida-api.enzojb.com.br';
const EMAIL    = __ENV.EMAIL   || 'teste@provavida.com';
const SENHA    = __ENV.SENHA   || 'Senha@123';

// Métricas customizadas
const loginDuration   = new Trend('login_duration', true);
const loginSuccessRate = new Rate('login_success_rate');

export const options = {
  stages: [
    { duration: '30s', target: 5  }, // rampa de subida
    { duration: '1m',  target: 10 }, // carga sustentada
    { duration: '20s', target: 0  }, // rampa de descida
  ],
  thresholds: {
    // BCrypt é lento por design — p95 < 2s é aceitável para login
    'login_duration': ['p(95)<2000'],
    'login_success_rate': ['rate>0.95'],
    'http_req_failed': ['rate<0.05'],
  },
};

export default function () {
  const payload = JSON.stringify({ email: EMAIL, senha: SENHA });
  const params  = { headers: { 'Content-Type': 'application/json' } };

  const res = http.post(`${BASE_URL}/auth/login`, payload, params);

  const ok = check(res, {
    'status 200':      (r) => r.status === 200,
    'token presente':  (r) => r.json('token') !== undefined,
    'expiraEm presente': (r) => r.json('expiraEm') !== undefined,
  });

  loginDuration.add(res.timings.duration);
  loginSuccessRate.add(ok);

  sleep(1);
}
