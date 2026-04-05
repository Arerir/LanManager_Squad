import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getSeats, createSeatsGrid, assignSeat, unassignSeat } from '../api/seats';
import type { SeatDto } from '../api/seats';
import { getAttendance } from '../api/attendance';
import type { AttendanceDto } from '../api/attendance';
import { getUser } from '../api/auth';

export function SeatingPage() {
  const [searchParams] = useSearchParams();
  const eventId = searchParams.get('eventId') ?? '';
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
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <h1 style={{ margin: 0 }}>Seating Map</h1>
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

      <div style={{ display: 'flex', gap: 24, alignItems: 'flex-start' }}>
        {/* SVG Map */}
        <div style={{ flex: 1, overflowX: 'auto' }}>
          {seats.length === 0 && !loading ? (
            <div style={{ color: '#aaa', padding: '3rem', textAlign: 'center', border: '2px dashed #333', borderRadius: 8 }}>
              No seats configured.{canSetup ? ' Use ⚙ Setup Grid to create seats.' : ''}
            </div>
          ) : (
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
          )}
        </div>

        {/* Right panel */}
        <div style={{ width: 280, flexShrink: 0 }}>
          {selectedSeat && (
            <div style={{ background: '#1a1a2e', borderRadius: 8, padding: '1rem', marginBottom: '1rem', border: `1px solid ${selectedSeat.assignedUserId ? '#c0392b' : '#3498db'}` }}>
              <strong>Seat {selectedSeat.label}</strong>
              {selectedSeat.assignedUserId ? (
                <div style={{ marginTop: 8 }}>
                  <span style={{ color: '#aaa', fontSize: '0.9rem' }}>Assigned: </span>
                  <strong>{selectedSeat.assignedUserName}</strong>
                  {canManage && (
                    <button onClick={handleUnassign}
                      style={{ display: 'block', marginTop: 8, padding: '4px 12px', background: '#c0392b', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: '0.85rem' }}>
                      Unassign
                    </button>
                  )}
                </div>
              ) : (
                <p style={{ color: '#aaa', fontSize: '0.85rem', margin: '4px 0 0' }}>
                  {canManage ? 'Click an attendee to assign.' : 'Empty seat'}
                </p>
              )}
            </div>
          )}

          <h3 style={{ margin: '0 0 0.75rem' }}>Checked-in Attendees</h3>
          <div style={{ maxHeight: 500, overflowY: 'auto' }}>
            {attendees.length === 0 && <p style={{ color: '#aaa', fontSize: '0.9rem' }}>No attendees checked in.</p>}
            {attendees.map(a => {
              const hasSeat = seats.find(s => s.assignedUserId === a.userId);
              return (
                <div key={a.userId}
                  onClick={() => { if (canManage && selectedSeat && !selectedSeat.assignedUserId) handleAssign(a); }}
                  style={{
                    padding: '8px 12px', borderRadius: 6, marginBottom: 4,
                    background: hasSeat ? '#1a3a1a' : '#1a1a2e',
                    cursor: canManage && selectedSeat && !selectedSeat.assignedUserId ? 'pointer' : 'default',
                    border: '1px solid #333', display: 'flex', justifyContent: 'space-between', alignItems: 'center'
                  }}>
                  <span>{a.userName}</span>
                  {hasSeat && <span style={{ fontSize: '0.8rem', color: '#2ecc71' }}>Seat {hasSeat.label}</span>}
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}
