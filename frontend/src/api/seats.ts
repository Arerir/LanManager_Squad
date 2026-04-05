import { apiFetch } from './apiClient';
import { config } from '../config';

const BASE = `${config.apiUrl}/api/events`;

export interface SeatDto {
  id: string; eventId: string; row: number; column: number;
  label: string; assignedUserId?: string; assignedUserName?: string; assignedAt?: string;
}

export async function getSeats(eventId: string): Promise<SeatDto[]> {
  const res = await apiFetch(`${BASE}/${eventId}/seats`);
  if (!res.ok) throw new Error('Failed to fetch seats');
  return res.json();
}
export async function createSeatsGrid(eventId: string, rows: number, columns: number): Promise<SeatDto[]> {
  const res = await apiFetch(`${BASE}/${eventId}/seats/grid`, { method: 'POST', body: JSON.stringify({ rows, columns }) });
  if (!res.ok) throw new Error('Failed to create grid');
  return res.json();
}
export async function assignSeat(eventId: string, seatId: string, userId: string, userName: string): Promise<SeatDto> {
  const res = await apiFetch(`${BASE}/${eventId}/seats/${seatId}/assign`, { method: 'PUT', body: JSON.stringify({ userId, userName }) });
  if (!res.ok) { const err = await res.json().catch(() => ({ message: 'Assign failed' })); throw new Error(err.message ?? 'Assign failed'); }
  return res.json();
}
export async function unassignSeat(eventId: string, seatId: string): Promise<void> {
  const res = await apiFetch(`${BASE}/${eventId}/seats/${seatId}/assign`, { method: 'DELETE' });
  if (!res.ok) throw new Error('Unassign failed');
}
