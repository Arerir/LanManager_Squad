import { apiFetch } from './apiClient';
import { config } from '../config';

const BASE = `${config.apiUrl}/api/events`;

export interface TournamentDto {
  id: string; eventId: string; name: string; format: string;
  status: string; createdAt: string; participantCount: number;
}
export interface MatchDto {
  id: string; round: number; matchNumber: number;
  player1Id?: string; player1Name?: string;
  player2Id?: string; player2Name?: string;
  winnerId?: string; winnerName?: string; status: string;
}
export interface RoundDto { round: number; roundName: string; matches: MatchDto[]; }
export interface BracketDto { tournamentId: string; name: string; status: string; rounds: RoundDto[]; }

export async function getTournaments(eventId: string): Promise<TournamentDto[]> {
  const res = await apiFetch(`${BASE}/${eventId}/tournaments`);
  if (!res.ok) throw new Error('Failed to fetch tournaments');
  return res.json();
}

export async function getBracket(eventId: string, tid: string): Promise<BracketDto> {
  const res = await apiFetch(`${BASE}/${eventId}/tournaments/${tid}/bracket`);
  if (!res.ok) throw new Error('Failed to fetch bracket');
  return res.json();
}

export async function createTournament(eventId: string, name: string, participants: {userId: string, displayName: string}[]): Promise<TournamentDto> {
  const res = await apiFetch(`${BASE}/${eventId}/tournaments`, { method: 'POST', body: JSON.stringify({ name, participants }) });
  if (!res.ok) throw new Error('Failed to create tournament');
  return res.json();
}

export async function submitResult(eventId: string, tid: string, matchId: string, winnerId: string): Promise<MatchDto> {
  const res = await apiFetch(`${BASE}/${eventId}/tournaments/${tid}/matches/${matchId}/result`, { method: 'PUT', body: JSON.stringify({ winnerId }) });
  if (!res.ok) throw new Error('Failed to submit result');
  return res.json();
}
