import { config } from '../config';

export interface AttendanceDto {
  userId: string;
  userName: string;
  checkedInAt: string;
}

const BASE = `${config.apiUrl}/api/events`;

export async function getAttendance(eventId: string): Promise<AttendanceDto[]> {
  const res = await fetch(`${BASE}/${eventId}/attendance`);
  if (!res.ok) throw new Error(`Failed to fetch attendance: ${res.statusText}`);
  return res.json();
}
