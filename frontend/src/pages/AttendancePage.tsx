import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { config } from '../config';
import { getAttendance } from '../api/attendance';
import type { AttendanceDto } from '../api/attendance';
import { getOutside, getDoorLog } from '../api/doorpass';
import type { OutsideUserDto, DoorPassDto } from '../api/doorpass';
import { useEventContext } from '../context/EventContext';

interface CheckedInBroadcast {
  eventId: string;
  userId: string;
  userName: string;
  name: string;
  checkedInAt: string;
}

interface CheckedOutBroadcast {
  eventId: string;
  userId: string;
  userName: string;
  checkedOutAt: string;
}

function elapsed(isoDate: string): string {
  const mins = Math.floor((Date.now() - new Date(isoDate).getTime()) / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  return `${Math.floor(mins / 60)}h ${mins % 60}m ago`;
}

// ─── Live Tab ────────────────────────────────────────────────────────────────

function LiveAttendanceTab({ eventId }: { eventId: string }) {
  const [attendees, setAttendees] = useState<AttendanceDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hubStatus, setHubStatus] = useState<string>('Connecting…');

  useEffect(() => {
    setLoading(true);
    setError(null);
    getAttendance(eventId)
      .then(setAttendees)
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false));
  }, [eventId]);

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${config.apiUrl}/hubs/attendance`)
      .withAutomaticReconnect()
      .build();

    connection.on('UserCheckedIn', (broadcast: CheckedInBroadcast) => {
      if (broadcast.eventId !== eventId) return;
      setAttendees(prev => {
        if (prev.some(a => a.userId === broadcast.userId)) return prev;
        return [...prev, {
          userId: broadcast.userId,
          userName: broadcast.userName,
          name: broadcast.name,
          checkedInAt: broadcast.checkedInAt,
        }];
      });
    });

    connection.on('UserCheckedOut', (broadcast: CheckedOutBroadcast) => {
      if (broadcast.eventId !== eventId) return;
      setAttendees(prev => prev.filter(a => a.userId !== broadcast.userId));
    });

    connection.start()
      .then(() => setHubStatus('Live'))
      .catch(() => setHubStatus('Disconnected'));

    connection.onreconnecting(() => setHubStatus('Reconnecting…'));
    connection.onreconnected(() => setHubStatus('Live'));
    connection.onclose(() => {
      if (connection.state !== HubConnectionState.Connecting) {
        setHubStatus('Disconnected');
      }
    });

    return () => { connection.stop(); };
  }, [eventId]);

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem' }}>
        <h2 style={{ margin: 0 }}>Live Attendance</h2>
        <span style={{
          background: hubStatus === 'Live' ? '#2a7a2a' : '#555',
          color: '#fff', borderRadius: '999px', padding: '2px 12px',
          fontSize: '0.8rem', fontWeight: 600,
        }}>
          {hubStatus}
        </span>
        <span style={{
          background: '#1a1a2e', color: '#7eb3ff', borderRadius: '999px',
          padding: '2px 12px', fontSize: '0.85rem', fontWeight: 600,
        }}>
          {attendees.length} checked in
        </span>
      </div>

      {loading && <p style={{ color: '#aaa' }}>Loading attendance…</p>}
      {error && <p style={{ color: '#f66' }}>Error: {error}</p>}
      {!loading && !error && attendees.length === 0 && (
        <p style={{ color: '#aaa' }}>0 people checked in.</p>
      )}

      {attendees.length > 0 && (
        <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #333', textAlign: 'left' }}>
              <th style={{ padding: '8px 12px' }}>Name</th>
              <th style={{ padding: '8px 12px' }}>Checked In At</th>
            </tr>
          </thead>
          <tbody>
            {attendees.map(a => (
              <tr key={a.userId} style={{ borderBottom: '1px solid #222' }}>
                <td style={{ padding: '8px 12px' }}>{a.userName}</td>
                <td style={{ padding: '8px 12px', color: '#aaa', fontSize: '0.9rem' }}>
                  {new Date(a.checkedInAt).toLocaleTimeString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      )}
    </div>
  );
}

// ─── Outside Now Tab ─────────────────────────────────────────────────────────

function OutsideNowTab({ eventId }: { eventId: string }) {
  const [users, setUsers] = useState<OutsideUserDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    function fetchData() {
      setLoading(true);
      getOutside(eventId)
        .then(data => { if (!cancelled) setUsers(data); })
        .catch((err: Error) => { if (!cancelled) setError(err.message); })
        .finally(() => { if (!cancelled) setLoading(false); });
    }

    fetchData();
    const timer = setInterval(fetchData, 30000);
    return () => { cancelled = true; clearInterval(timer); };
  }, [eventId]);

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem' }}>
        <h2 style={{ margin: 0 }}>Outside Now</h2>
        <span style={{
          background: '#1a1a2e', color: '#7eb3ff', borderRadius: '999px',
          padding: '2px 12px', fontSize: '0.85rem', fontWeight: 600,
        }}>
          {users.length} outside
        </span>
      </div>

      {loading && <p style={{ color: '#aaa' }}>Loading…</p>}
      {error && <p style={{ color: '#f66' }}>Error: {error}</p>}
      {!loading && !error && users.length === 0 && (
        <p style={{ color: '#aaa' }}>Everyone is inside.</p>
      )}

      {users.length > 0 && (
        <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #333', textAlign: 'left' }}>
              <th style={{ padding: '8px 12px' }}>Name</th>
              <th style={{ padding: '8px 12px' }}>Exited At</th>
              <th style={{ padding: '8px 12px' }}>Duration</th>
            </tr>
          </thead>
          <tbody>
            {users.map(u => (
              <tr key={u.userId} style={{ borderBottom: '1px solid #222' }}>
                <td style={{ padding: '8px 12px' }}>{u.userName}</td>
                <td style={{ padding: '8px 12px', color: '#aaa', fontSize: '0.9rem' }}>
                  {new Date(u.exitedAt).toLocaleTimeString()}
                </td>
                <td style={{ padding: '8px 12px', color: '#f0a500', fontSize: '0.9rem' }}>
                  {elapsed(u.exitedAt)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      )}
    </div>
  );
}

// ─── Door Log Tab ─────────────────────────────────────────────────────────────

const PAGE_SIZE = 20;

function DoorLogTab({ eventId }: { eventId: string }) {
  const [records, setRecords] = useState<DoorPassDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  useEffect(() => {
    setLoading(true);
    setError(null);
    getDoorLog(eventId)
      .then(setRecords)
      .catch((err: Error) => setError(err.message))
      .finally(() => setLoading(false));
  }, [eventId]);

  const totalPages = Math.max(1, Math.ceil(records.length / PAGE_SIZE));
  const pageRecords = records.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem' }}>
        <h2 style={{ margin: 0 }}>Door Log</h2>
        <span style={{
          background: '#1a1a2e', color: '#7eb3ff', borderRadius: '999px',
          padding: '2px 12px', fontSize: '0.85rem', fontWeight: 600,
        }}>
          {records.length} entries
        </span>
      </div>

      {loading && <p style={{ color: '#aaa' }}>Loading…</p>}
      {error && <p style={{ color: '#f66' }}>Error: {error}</p>}
      {!loading && !error && records.length === 0 && (
        <p style={{ color: '#aaa' }}>No door passes recorded.</p>
      )}

      {pageRecords.length > 0 && (
        <>
          <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid #333', textAlign: 'left' }}>
                <th style={{ padding: '8px 12px' }}>Direction</th>
                <th style={{ padding: '8px 12px' }}>Name</th>
                <th style={{ padding: '8px 12px' }}>Time</th>
              </tr>
            </thead>
            <tbody>
              {pageRecords.map(r => (
                <tr key={r.id} style={{ borderBottom: '1px solid #222' }}>
                  <td style={{ padding: '8px 12px' }}>
                    <span style={{
                      color: r.direction === 'Exit' ? '#e74c3c' : '#2ecc71',
                      fontWeight: 600,
                    }}>
                      {r.direction === 'Exit' ? '🚪 Exit' : '↩ Entry'}
                    </span>
                  </td>
                  <td style={{ padding: '8px 12px' }}>{r.userName}</td>
                  <td style={{ padding: '8px 12px', color: '#aaa', fontSize: '0.9rem' }}>
                    {new Date(r.scannedAt).toLocaleTimeString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          </div>

          {totalPages > 1 && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginTop: 16 }}>
              <button
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
                style={{ padding: '6px 14px', cursor: page === 1 ? 'default' : 'pointer', opacity: page === 1 ? 0.4 : 1 }}
              >
                ← Prev
              </button>
              <span style={{ color: '#aaa', fontSize: '0.9rem' }}>
                Page {page} of {totalPages}
              </span>
              <button
                onClick={() => setPage(p => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                style={{ padding: '6px 14px', cursor: page === totalPages ? 'default' : 'pointer', opacity: page === totalPages ? 0.4 : 1 }}
              >
                Next →
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export function AttendancePage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const eventId = searchParams.get('eventId') ?? '';
  const tab = (searchParams.get('tab') ?? 'live') as 'live' | 'outside' | 'doorlog';
  const { setSelectedEventId } = useEventContext();

  useEffect(() => {
    if (eventId) setSelectedEventId(eventId);
  }, [eventId, setSelectedEventId]);

  const setTab = (t: string) =>
    setSearchParams(p => { p.set('tab', t); return p; });

  if (!eventId) {
    return (
      <div style={{ padding: '2rem' }}>
        <p style={{ color: '#aaa' }}>Select an event to view attendance.</p>
      </div>
    );
  }

  return (
    <div>
      <div style={{ display: 'flex', borderBottom: '2px solid #eee', marginBottom: 16 }}>
        {(['live', 'outside', 'doorlog'] as const).map(t => (
          <button key={t} onClick={() => setTab(t)}
            style={{
              padding: '8px 20px',
              fontWeight: tab === t ? 'bold' : 'normal',
              borderBottom: tab === t ? '2px solid #3498db' : 'none',
              background: 'none', border: 'none', cursor: 'pointer',
            }}>
            {t === 'live' ? '🟢 Live' : t === 'outside' ? '🚪 Outside Now' : '📋 Door Log'}
          </button>
        ))}
      </div>

      {tab === 'live' && <LiveAttendanceTab eventId={eventId} />}
      {tab === 'outside' && <OutsideNowTab eventId={eventId} />}
      {tab === 'doorlog' && <DoorLogTab eventId={eventId} />}
    </div>
  );
}

