import { config } from '../config';
import { apiFetch } from './apiClient';
import type { UserDto } from './users';

export interface EventDto {
  id: string;
  name: string;
  description?: string;
  location?: string;
  startDate: string;
  endDate: string;
  capacity: number;
  status: 'Draft' | 'Published' | 'Active' | 'Closed';
  createdAt: string;
}

export interface CreateEventRequest {
  name: string;
  description?: string;
  location?: string;
  startDate: string;
  endDate: string;
  capacity: number;
  status: 'Draft' | 'Published' | 'Active' | 'Closed';
}

export type UpdateEventRequest = CreateEventRequest;

export type EventStatus = 'All' | 'Draft' | 'Published' | 'Active' | 'Closed';

const BASE = `${config.apiUrl}/api/events`;

export async function getEvents(status?: EventStatus, sort?: string): Promise<EventDto[]> {
  const params = new URLSearchParams();
  if (status && status !== 'All') params.set('status', status);
  if (sort) params.set('sort', sort);
  const query = params.toString();
  const res = await apiFetch(query ? `${BASE}?${query}` : BASE);
  if (!res.ok) throw new Error(`Failed to fetch events: ${res.statusText}`);
  return res.json();
}

export async function getEvent(id: string): Promise<EventDto> {
  const res = await apiFetch(`${BASE}/${id}`);
  if (!res.ok) throw new Error(`Failed to fetch event: ${res.statusText}`);
  return res.json();
}

export async function createEvent(data: CreateEventRequest): Promise<EventDto> {
  const res = await apiFetch(BASE, {
    method: 'POST',
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Failed to create event: ${res.statusText}`);
  }
  return res.json();
}

export async function updateEvent(id: string, data: UpdateEventRequest): Promise<EventDto> {
  const res = await apiFetch(`${BASE}/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Failed to update event: ${res.statusText}`);
  }
  return res.json();
}

export async function deleteEvent(id: string): Promise<void> {
  const res = await apiFetch(`${BASE}/${id}`, { method: 'DELETE' });
  if (!res.ok) throw new Error(`Failed to delete event: ${res.statusText}`);
}

export async function getEventAttendees(eventId: string): Promise<UserDto[]> {
  const res = await apiFetch(`${BASE}/${eventId}/attendees`);
  if (!res.ok) throw new Error(`Failed to fetch attendees: ${res.statusText}`);
  return res.json();
}

export async function registerForEvent(eventId: string, userId: string): Promise<void> {
  const res = await apiFetch(`${BASE}/${eventId}/register`, {
    method: 'POST',
    body: JSON.stringify({ userId }),
  });
  if (!res.ok) throw new Error(`Failed to register for event: ${res.statusText}`);
}