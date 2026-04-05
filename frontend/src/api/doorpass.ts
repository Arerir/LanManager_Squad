import { config } from '../config';
import { apiFetch } from './apiClient';

export interface OutsideUserDto {
  userId: string;
  userName: string;
  exitedAt: string;
}

export interface DoorPassDto {
  id: string;
  eventId: string;
  userId: string;
  userName: string;
  direction: 'Exit' | 'Entry';
  scannedAt: string;
}

const BASE = `${config.apiUrl}/api/events`;

export async function getOutside(eventId: string): Promise<OutsideUserDto[]> {
  const res = await apiFetch(`${BASE}/${eventId}/outside`);
  if (!res.ok) throw new Error(`Failed to fetch outside users: ${res.statusText}`);
  return res.json();
}

export async function getDoorLog(eventId: string, page = 1, pageSize = 50): Promise<DoorPassDto[]> {
  const res = await apiFetch(`${BASE}/${eventId}/door-log?page=${page}&pageSize=${pageSize}`);
  if (!res.ok) throw new Error(`Failed to fetch door log: ${res.statusText}`);
  return res.json();
}