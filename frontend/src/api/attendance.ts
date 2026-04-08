import { config } from '../config';
import { apiFetch } from './apiClient';

export interface AttendanceDto {
  userId: string;
  userName: string;
  name: string;
  checkedInAt: string;
}

const BASE = `${config.apiUrl}/api/events`;

export async function getAttendance(eventId: string): Promise<AttendanceDto[]> {
  const res = await apiFetch(`${BASE}/${eventId}/attendance`);
  if (!res.ok) throw new Error(`Failed to fetch attendance: ${res.statusText}`);
  return res.json();
}