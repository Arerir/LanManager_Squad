import { config } from '../config';

export interface UserDto {
  id: string;
  name: string;
  userName: string;
  email: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  userName: string;
  name: string;
}

const BASE = `${config.apiUrl}/api/users`;

export async function registerUser(data: RegisterRequest): Promise<UserDto> {
  const res = await fetch(`${BASE}/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Registration failed: ${res.statusText}`);
  }
  return res.json();
}

export async function getUser(id: string): Promise<UserDto> {
  const res = await fetch(`${BASE}/${id}`);
  if (!res.ok) throw new Error(`Failed to fetch user: ${res.statusText}`);
  return res.json();
}

export async function getUsers(page = 1, pageSize = 20): Promise<UserDto[]> {
  const res = await fetch(`${BASE}?page=${page}&pageSize=${pageSize}`);
  if (!res.ok) throw new Error(`Failed to fetch users: ${res.statusText}`);
  return res.json();
}

export async function loginUser(email: string, password: string): Promise<UserDto> {
  const res = await fetch(`${config.apiUrl}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Login failed: ${res.statusText}`);
  }
  return res.json();
}
