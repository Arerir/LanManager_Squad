import { config } from '../config';
import { apiFetch } from './apiClient';

export interface EquipmentLoanDto {
  id: string;
  equipmentId: string;
  equipmentName: string;
  userId: string;
  userName: string;
  eventId: string;
  borrowedAt: string;
  returnedAt: string | null;
}

export interface EquipmentDto {
  id: string;
  name: string;
  type: string;
  qrCode: string;
  notes: string | null;
  isAvailable: boolean;
  activeLoan: EquipmentLoanDto | null;
}

const BASE = `${config.apiUrl}/api/equipment`;

export async function getEquipment(): Promise<EquipmentDto[]> {
  const res = await apiFetch(BASE);
  if (!res.ok) throw new Error(`Failed to fetch equipment: ${res.statusText}`);
  return res.json();
}

export async function createEquipment(data: {
  name: string;
  type: string;
  qrCode: string;
  notes?: string;
}): Promise<EquipmentDto> {
  const res = await apiFetch(BASE, {
    method: 'POST',
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error(`Failed to create equipment: ${res.statusText}`);
  return res.json();
}

export async function updateEquipment(
  id: string,
  data: { name: string; type: string; notes?: string }
): Promise<EquipmentDto> {
  const res = await apiFetch(`${BASE}/${id}`, {
    method: 'PATCH',
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error(`Failed to update equipment: ${res.statusText}`);
  return res.json();
}

export async function returnEquipment(id: string): Promise<void> {
  const res = await apiFetch(`${BASE}/${id}/return`, { method: 'POST' });
  if (!res.ok) throw new Error(`Failed to return equipment: ${res.statusText}`);
}
