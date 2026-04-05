import { config } from '../config';

export interface LoginResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
  roles: string[];
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const res = await fetch(`${config.apiUrl}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: 'Login failed' }));
    throw new Error(err.message ?? 'Login failed');
  }
  return res.json();
}

export function getToken(): string | null {
  return localStorage.getItem('jwt_token');
}

export function setToken(token: string): void {
  localStorage.setItem('jwt_token', token);
}

export function clearToken(): void {
  localStorage.removeItem('jwt_token');
  localStorage.removeItem('jwt_user');
}

export function getUser(): LoginResponse | null {
  const raw = localStorage.getItem('jwt_user');
  return raw ? JSON.parse(raw) : null;
}

export function setUser(user: LoginResponse): void {
  localStorage.setItem('jwt_user', JSON.stringify(user));
}
