import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getSeats, createSeatsGrid, assignSeat, unassignSeat } from '../api/seats';
import type { SeatDto } from '../api/seats';
import { getAttendance } from '../api/attendance';
import type { AttendanceDto } from '../api/attendance';
import { getUser } from '../api/auth';
import { useEventContext } from '../context/EventContext';

export function SeatingPage() {
  const [searchParams] = useSearchParams();
  const eventId = searchParams.get('eventId') ?? '';
  const { setSelectedEventId } = useEventContext();
  const [seats, setSeats] = useState<SeatDto[]>([]);
  const [attendees, setAttendees] = useState<AttendanceDto[]>([]);
  const [selectedSeat, setSelectedSeat] = useState<SeatDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showSetup, setShowSetup] = useState(false);
  const [setupRows, setSetupRows] = useState(5);
  const [setupCols, setSetupCols] = useState(10);

  const currentUser = getUser();
  const canManage = currentUser?.roles.some(r => ['Admin', 'Organizer', 'Operator'].includes(r));
  const canSetup = currentUser?.roles.some(r => ['Admin', 'Organizer'].includes(r));

  useEffect(() => {
    if (eventId) setSelectedEventId(eventId);
  }, [eventId, setSelectedEventId]);

  useEffect(() => {
    if (!eventId) return;
    setLoading(true);
    Promise.all([
      getSeats(eventId).then(setSeats),
      getAttendance(eventId).then(setAttendees)
    ]).catch(e => setError(e instanceof Error ? e.message : 'Load failed'))
      .finally(() => setLoading(false));
  }, [eventId]);

  if (!eventId) return <div style={{ padding: '2rem', color: '#aaa' }}>Select an event to view seating.</div>;

  const maxRow = seats.length > 0 ? Math.max(...seats.map(s => s.row)) : -1;
  const maxCol = seats.length > 0 ? Math.max(...seats.map(s => s.column)) : -1;
  const SEAT_W = 60, SEAT_H = 50, GAP = 8;

  const seatAt = (r: number, c: number) => seats.find(s => s.row === r && s.column === c);

  async function handleSeatClick(seat: SeatDto) {
    setSelectedSeat(prev => prev?.id === seat.id ? null : seat);
  }

  async function handleAssign(a: AttendanceDto) {
    if (!selectedSeat || selectedSeat.assignedUserId || !canManage) return;
    try {
      const updated = await assignSeat(eventId, selectedSeat.id, a.userId, a.userName);
      setSeats(prev => prev.map(s => {
        if (s.assignedUserId === a.userId) return { ...s, assignedUserId: undefined, assignedUserName: undefined, assignedAt: undefined };
        if (s.id === updated.id) return updated;
        return s;
      }));
      setSelectedSeat(updated);
      setError(null);
    } catch (e: unknown) { setError(e instanceof Error ? e.message : 'Assign failed'); }
  }

  async function handleUnassign() {
    if (!selectedSeat?.assignedUserId || !canManage) return;
    try {
      await unassignSeat(eventId, selectedSeat.id);
      const cleared = { ...selectedSeat, assignedUserId: undefined, assignedUserName: undefined, assignedAt: undefined };
      setSeats(prev => prev.map(s => s.id === selectedSeat.id ? cleared : s));
      setSelectedSeat(cleared);
      setError(null);
    } catch (e: unknown) { setError(e instanceof Error ? e.message : 'Unassign failed'); }
  }

  async function handleCreateGrid() {
    try {
      const newSeats = await createSeatsGrid(eventId, setupRows, setupCols);
      setSeats(newSeats); setShowSetup(false); setSelectedSeat(null); setError(null);
    } catch (e: unknown) { setError(e instanceof Error ? e.message : 'Setup failed'); }
  }

  const assignedCount = seats.filter(s => s.assignedUserId).length;

  return (
    <div>
      {/* ── Page header ──────────────────────────────────────────────────────── */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <h1 className="page-title">Seating Map</h1>
        <span style={{ color: '#aaa', fontSize: '0.9rem' }}>{assignedCount}/{seats.length} seats assigned</span>
        {canSetup && (
          <button onClick={() => setShowSetup(v => !v)}
            style={{ padding: '6px 14px', background: '#555', color: '#fff', border: 'none', borderRadius: 6, cursor: 'pointer', marginLeft: 'auto' }}>
            ⚙ Setup Grid
          </button>
        )}
      </div>

      {showSetup && (
        <div style={{ background: '#1a1a2e', borderRadius: 8, padding: '1rem', marginBottom: '1.5rem', display: 'flex', gap: '1rem', alignItems: 'center', flexWrap: 'wrap' }}>
          <label style={{ color: '#aaa' }}>
            Rows:&nbsp;
            <input type="number" min={1} max={26} value={setupRows} onChange={e => setSetupRows(+e.target.value)}
              style={{ width: 60, padding: 6, borderRadius: 4, border: '1px solid #333', background: '#0d0d1a', color: '#fff' }} />
          </label>
          <label style={{ color: '#aaa' }}>
            Cols:&nbsp;
            <input type="number" min={1} max={50} value={setupCols} onChange={e => setSetupCols(+e.target.value)}
              style={{ width: 60, padding: 6, borderRadius: 4, border: '1px solid #333', background: '#0d0d1a', color: '#fff' }} />
          </label>
          <button onClick={handleCreateGrid}
            style={{ padding: '6px 16px', background: '#e74c3c', color: '#fff', border: 'none', borderRadius: 6, cursor: 'pointer' }}>
            Generate (replaces existing)
          </button>
        </div>
      )}

      {error && <p style={{ color: '#f66', marginBottom: '1rem' }}>Error: {error}</p>}
      {loading && <p style={{ color: '#aaa' }}>Loading…</p>}

      {/* ── Dedicated seating panel (full-width) ─────────────────────────────── */}
      <div style={{
        background: '#12122a',
        border: '1px solid #2a2a4a',
        borderRadius: 10,
        padding: '1.5rem',
        marginBottom: '2rem',
        overflowX: 'auto',
      }}>
        {seats.length === 0 && !loading ? (
          <div style={{ color: '#aaa', padding: '3rem', textAlign: 'center', border: '2px dashed #333', borderRadius: 8 }}>
            No seats configured.{canSetup ? ' Use ⚙ Setup Grid to create seats.' : ''}
          </div>
        ) : (
          <div style={{ display: 'flex', justifyContent: 'center' }}>
            <svg width={(maxCol + 1) * (SEAT_W + GAP)} height={(maxRow + 1) * (SEAT_H + GAP) + 24} style={{ display: 'block' }}>
              <text x={(maxCol + 1) * (SEAT_W + GAP) / 2} y={14} textAnchor="middle" fill="#555" fontSize={11} letterSpacing={4}>▲ STAGE ▲</text>
              {Array.from({ length: maxRow + 1 }, (_, r) =>
                Array.from({ length: maxCol + 1 }, (_, c) => {
                  const seat = seatAt(r, c);
                  if (!seat) return null;
                  const x = c * (SEAT_W + GAP);
                  const y = r * (SEAT_H + GAP) + 22;
                  const isSelected = selectedSeat?.id === seat.id;
                  const isOccupied = !!seat.assignedUserId;
                  const fill = isSelected ? '#1a5276' : isOccupied ? '#8b0000' : '#2d2d4e';
                  const stroke = isSelected ? '#3498db' : isOccupied ? '#c0392b' : '#444';
                  return (
                    <g key={seat.id} onClick={() => handleSeatClick(seat)} style={{ cursor: canManage ? 'pointer' : 'default' }}>
                      <rect x={x} y={y} width={SEAT_W} height={SEAT_H} rx={4} fill={fill} stroke={stroke} strokeWidth={isSelected ? 2 : 1} />
                      <text x={x + SEAT_W / 2} y={y + 18} textAnchor="middle" fill="#fff" fontSize={11} fontWeight={600}>{seat.label}</text>
                      {isOccupied && (
                        <text x={x + SEAT_W / 2} y={y + 35} textAnchor="middle" fill="#ffaaaa" fontSize={9}>
                          {(seat.assignedUserName ?? '').split(' ')[0]}
                        </text>
                      )}
                    </g>
                  );
                })
              )}
            </svg>
          </div>
        )}

        {/* Selected seat callout — inline at bottom of the seating panel */}
        {selectedSeat && (
          <div style={{
            marginTop: '1.25rem',
            padding: '0.75rem 1rem',
            borderRadius: 8,
            background: '#1a1a2e',
            border: `1px solid ${selectedSeat.assignedUserId ? '#c0392b' : '#3498db'}`,
            display: 'flex', alignItems: 'center', gap: '1.5rem', flexWrap: 'wrap',
          }}>
            <strong style={{ fontSize: '1rem' }}>Seat {selectedSeat.label}</strong>
            {selectedSeat.assignedUserId ? (
              <>
                <span style={{ color: '#aaa', fontSize: '0.9rem' }}>
                  Assigned to <strong style={{ color: '#fff' }}>{selectedSeat.assignedUserName}</strong>
                </span>
                {canManage && (
                  <button onClick={handleUnassign}
                    style={{ padding: '4px 12px', background: '#c0392b', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: '0.85rem' }}>
                    Unassign
                  </button>
                )}
              </>
            ) : (
              <span style={{ color: '#aaa', fontSize: '0.85rem' }}>
                {canManage ? 'Select an attendee below to assign this seat.' : 'Empty seat'}
              </span>
            )}
          </div>
        )}
      </div>

      {/* ── Attendees section ────────────────────────────────────────────────── */}
      <div style={{
        background: '#12122a',
        border: '1px solid #2a2a4a',
        borderRadius: 10,
        padding: '1.5rem',
      }}>
        <h2 style={{ margin: '0 0 1rem', fontSize: '1.15rem' }}>Checked-in Attendees</h2>
        {attendees.length === 0 && <p style={{ color: '#aaa', fontSize: '0.9rem' }}>No attendees checked in.</p>}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(160px, 1fr))',
          gap: 8,
        }}>
          {attendees.map(a => {
            const hasSeat = seats.find(s => s.assignedUserId === a.userId);
            const isClickable = !!(canManage && selectedSeat && !selectedSeat.assignedUserId);
            return (
              <div key={a.userId}
                onClick={() => { if (isClickable) handleAssign(a); }}
                style={{
                  minHeight: 80,
                  padding: '12px 14px',
                  borderRadius: 8,
                  background: hasSeat ? '#1a3a1a' : '#3a0000',
                  cursor: isClickable ? 'pointer' : 'default',
                  border: isClickable ? '1px solid #3498db' : '1px solid #333',
                  display: 'flex',
                  flexDirection: 'column',
                  justifyContent: 'space-between',
                  transition: 'border-color 0.15s',
                }}>
                <div style={{
                  fontSize: '0.8rem',
                  fontWeight: 700,
                  textTransform: 'uppercase',
                  letterSpacing: '0.05em',
                  color: hasSeat ? '#2ecc71' : '#e74c3c',
                }}>
                  {hasSeat ? `Seat ${hasSeat.label}` : 'Unassigned'}
                </div>
                <div style={{ fontSize: '1rem', fontWeight: 500, color: '#fff' }}>
                  {a.userName}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
