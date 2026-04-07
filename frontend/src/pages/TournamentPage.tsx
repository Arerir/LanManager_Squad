import { useState, useEffect, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { config } from '../config';
import { getTournaments, getBracket, submitResult, createTournament } from '../api/tournaments';
import type { TournamentDto, BracketDto, MatchDto } from '../api/tournaments';
import { getUser } from '../api/auth';
import { getAttendance } from '../api/attendance';
import type { AttendanceDto } from '../api/attendance';
import { useEventContext } from '../context/EventContext';

export function TournamentPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const eventId = searchParams.get('eventId') ?? '';
  const tournamentId = searchParams.get('tournamentId');
  const { setSelectedEventId } = useEventContext();

  useEffect(() => {
    if (eventId) setSelectedEventId(eventId);
  }, [eventId, setSelectedEventId]);

  if (!eventId) return <div style={{ padding: '2rem', color: '#aaa' }}>Select an event to view tournaments.</div>;

  return tournamentId
    ? <BracketView eventId={eventId} tournamentId={tournamentId}
        onBack={() => setSearchParams(p => { p.delete('tournamentId'); return p; })} />
    : <TournamentListView eventId={eventId}
        onSelect={id => setSearchParams(p => { p.set('tournamentId', id); return p; })} />;
}

function TournamentListView({ eventId, onSelect }: { eventId: string; onSelect: (id: string) => void }) {
  const [tournaments, setTournaments] = useState<TournamentDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const user = getUser();
  const canManage = user?.roles.some(r => r === 'Admin' || r === 'Organizer');

  useEffect(() => {
    setLoading(true);
    getTournaments(eventId).then(setTournaments).catch(() => {}).finally(() => setLoading(false));
  }, [eventId]);

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <h1 className="page-title">Tournaments</h1>
        {canManage && (
          <button onClick={() => setShowCreate(true)}
            style={{ padding: '8px 16px', background: '#3498db', color: '#fff', border: 'none', borderRadius: 6, cursor: 'pointer' }}>
            + New Tournament
          </button>
        )}
      </div>
      {loading && <p style={{ color: '#aaa' }}>Loading…</p>}
      {!loading && tournaments.length === 0 && <p style={{ color: '#aaa' }}>No tournaments yet.</p>}
      {tournaments.map(t => (
        <div key={t.id} onClick={() => onSelect(t.id)}
          style={{ background: '#1a1a2e', borderRadius: 8, padding: '1rem 1.5rem', marginBottom: '1rem',
            cursor: 'pointer', display: 'flex', justifyContent: 'space-between', alignItems: 'center', border: '1px solid #333' }}>
          <div>
            <strong>{t.name}</strong>
            <span style={{ color: '#aaa', fontSize: '0.85rem', marginLeft: 12 }}>
              {t.participantCount} participants · {t.format}
            </span>
          </div>
          <span style={{ background: t.status === 'Completed' ? '#2ecc71' : '#f0a500', color: '#000',
            borderRadius: 999, padding: '2px 10px', fontSize: '0.8rem', fontWeight: 600 }}>
            {t.status}
          </span>
        </div>
      ))}
      {showCreate && (
        <CreateTournamentModal eventId={eventId}
          onCreated={t => { setTournaments(prev => [...prev, t]); setShowCreate(false); onSelect(t.id); }}
          onClose={() => setShowCreate(false)} />
      )}
    </div>
  );
}

function CreateTournamentModal({ eventId, onCreated, onClose }:
  { eventId: string; onCreated: (t: TournamentDto) => void; onClose: () => void }) {
  const [name, setName] = useState('');
  const [attendees, setAttendees] = useState<AttendanceDto[]>([]);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getAttendance(eventId)
      .then(a => setAttendees(a))
      .catch(() => {});
  }, [eventId]);

  const toggle = (id: string) => setSelected(prev => {
    const s = new Set(prev); s.has(id) ? s.delete(id) : s.add(id); return s;
  });

  async function handleCreate() {
    if (!name.trim() || selected.size < 2) return;
    setSaving(true); setError(null);
    try {
      const participants = attendees
        .filter(a => selected.has(a.userId))
        .map(a => ({ userId: a.userId, displayName: a.userName }));
      const t = await createTournament(eventId, name, participants);
      onCreated(t);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to create');
    } finally { setSaving(false); }
  }

  return (
    <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.7)', display: 'flex',
      alignItems: 'center', justifyContent: 'center', zIndex: 100 }}>
      <div style={{ background: '#1a1a2e', borderRadius: 12, padding: '2rem', width: 480,
        maxHeight: '80vh', overflowY: 'auto' }}>
        <h2 style={{ marginTop: 0 }}>New Tournament</h2>
        <input value={name} onChange={e => setName(e.target.value)} placeholder="Tournament name"
          style={{ width: '100%', padding: 10, borderRadius: 6, border: '1px solid #333',
            background: '#0d0d1a', color: '#fff', boxSizing: 'border-box', marginBottom: '1rem' }} />
        <p style={{ color: '#aaa', marginBottom: 8, fontSize: '0.9rem' }}>
          Select participants from checked-in attendees ({selected.size} selected, min 2):
        </p>
        {attendees.length === 0 && <p style={{ color: '#555', fontSize: '0.85rem' }}>No checked-in attendees.</p>}
        {attendees.map(a => (
          <label key={a.userId} style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '4px 0', cursor: 'pointer' }}>
            <input type="checkbox" checked={selected.has(a.userId)} onChange={() => toggle(a.userId)} />
            {a.userName}
          </label>
        ))}
        {error && <p style={{ color: '#f66', marginTop: 8 }}>{error}</p>}
        <div style={{ display: 'flex', gap: 8, marginTop: '1.5rem', justifyContent: 'flex-end' }}>
          <button onClick={onClose}
            style={{ padding: '8px 16px', background: '#333', color: '#fff', border: 'none', borderRadius: 6, cursor: 'pointer' }}>
            Cancel
          </button>
          <button onClick={handleCreate} disabled={saving || selected.size < 2 || !name.trim()}
            style={{ padding: '8px 16px', background: '#3498db', color: '#fff', border: 'none', borderRadius: 6,
              cursor: 'pointer', opacity: (selected.size < 2 || !name.trim()) ? 0.5 : 1 }}>
            {saving ? 'Creating…' : 'Create Bracket'}
          </button>
        </div>
      </div>
    </div>
  );
}

function BracketView({ eventId, tournamentId, onBack }:
  { eventId: string; tournamentId: string; onBack: () => void }) {
  const [bracket, setBracket] = useState<BracketDto | null>(null);
  const [loading, setLoading] = useState(false);
  const user = getUser();
  const canManage = user?.roles.some(r => r === 'Admin' || r === 'Organizer');

  const load = useCallback(() => {
    setLoading(true);
    getBracket(eventId, tournamentId).then(setBracket).catch(() => {}).finally(() => setLoading(false));
  }, [eventId, tournamentId]);

  useEffect(() => { load(); }, [load]);

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl(`${config.apiUrl}/hubs/tournament`)
      .withAutomaticReconnect()
      .build();

    conn.on('MatchResultUpdated', (updated: MatchDto) => {
      setBracket(prev => !prev ? prev : {
        ...prev,
        rounds: prev.rounds.map(r => ({
          ...r,
          matches: r.matches.map(m => m.id === updated.id ? updated : m)
        }))
      });
    });

    conn.start().then(() => conn.invoke('JoinTournament', tournamentId)).catch(() => {});
    return () => { conn.stop(); };
  }, [tournamentId]);

  async function handleSubmitResult(match: MatchDto, winnerId: string) {
    try { await submitResult(eventId, tournamentId, match.id, winnerId); }
    catch (e) { alert(e instanceof Error ? e.message : 'Failed'); }
  }

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem' }}>
        <button onClick={onBack}
          style={{ background: 'none', border: '1px solid #333', color: '#aaa', padding: '6px 12px', borderRadius: 6, cursor: 'pointer' }}>
          ← Back
        </button>
        <h1 className="page-title">{bracket?.name ?? 'Loading…'}</h1>
        {bracket && (
          <span style={{ background: bracket.status === 'Completed' ? '#2ecc71' : '#f0a500',
            color: '#000', borderRadius: 999, padding: '2px 10px', fontSize: '0.8rem', fontWeight: 600 }}>
            {bracket.status}
          </span>
        )}
      </div>
      {loading && <p style={{ color: '#aaa' }}>Loading bracket…</p>}
      {bracket && (
        <div style={{ display: 'flex', gap: 24, overflowX: 'auto', paddingBottom: '1rem', alignItems: 'flex-start' }}>
          {bracket.rounds.map(round => (
            <div key={round.round} style={{ minWidth: 220, flexShrink: 0 }}>
              <h3 style={{ textAlign: 'center', color: '#7eb3ff', marginBottom: '1rem',
                fontSize: '0.9rem', textTransform: 'uppercase', letterSpacing: 1 }}>
                {round.roundName}
              </h3>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
                {round.matches.map(match => (
                  <MatchCard key={match.id} match={match}
                    isFinal={round.roundName === 'Final'}
                    canManage={!!canManage}
                    onSubmit={handleSubmitResult} />
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function MatchCard({ match, isFinal, canManage, onSubmit }:
  { match: MatchDto; isFinal: boolean; canManage: boolean; onSubmit: (m: MatchDto, w: string) => void }) {
  const isCompleted = match.status === 'Completed' || match.status === 'Bye';

  const playerStyle = (playerId?: string): React.CSSProperties => ({
    padding: '8px 10px', borderRadius: 6,
    cursor: canManage && !isCompleted && playerId ? 'pointer' : 'default',
    background: match.winnerId && match.winnerId === playerId ? '#2a7a2a' : '#0d0d1a',
    color: match.winnerId && match.winnerId === playerId ? '#fff' : (playerId ? '#ddd' : '#555'),
    fontWeight: match.winnerId === playerId ? 700 : 400,
    border: `1px solid ${match.winnerId === playerId ? '#2ecc71' : '#333'}`,
    display: 'flex', alignItems: 'center', justifyContent: 'space-between',
    opacity: match.status === 'Bye' ? 0.5 : 1,
    transition: 'background 0.15s',
  });

  return (
    <div style={{ background: '#1a1a2e', borderRadius: 8, overflow: 'hidden', border: '1px solid #333' }}>
      {isFinal && match.winnerId && (
        <div style={{ textAlign: 'center', padding: '4px', background: '#f0a500', color: '#000', fontWeight: 700, fontSize: '0.8rem' }}>
          🏆 CHAMPION
        </div>
      )}
      <div style={playerStyle(match.player1Id ?? undefined)}
        onClick={() => { if (canManage && !isCompleted && match.player1Id) onSubmit(match, match.player1Id); }}>
        <span>{match.player1Name ?? 'TBD'}</span>
        {match.winnerId === match.player1Id && <span>✓</span>}
      </div>
      <div style={{ textAlign: 'center', color: '#555', fontSize: '0.75rem', padding: '2px 0' }}>vs</div>
      <div style={playerStyle(match.player2Id ?? undefined)}
        onClick={() => { if (canManage && !isCompleted && match.player2Id) onSubmit(match, match.player2Id); }}>
        <span>{match.player2Name ?? 'TBD'}</span>
        {match.winnerId === match.player2Id && <span>✓</span>}
      </div>
    </div>
  );
}
