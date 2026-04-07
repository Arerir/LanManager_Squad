import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getEvents } from '../api/events';
import type { EventDto, EventStatus } from '../api/events';

const STATUS_COLORS: Record<string, { bg: string; color: string }> = {
  Draft:     { bg: 'rgba(90,90,128,0.2)',   color: '#9ca3c8' },
  Published: { bg: 'rgba(0,212,255,0.15)',  color: '#00d4ff' },
  Active:    { bg: 'rgba(0,230,118,0.15)',  color: '#00e676' },
  Closed:    { bg: 'rgba(255,56,96,0.15)',  color: '#ff3860' },
};

function StatusBadge({ status }: { status: string }) {
  const colors = STATUS_COLORS[status] ?? STATUS_COLORS.Draft;
  return (
    <span style={{
      background: colors.bg,
      color: colors.color,
      padding: '2px 10px',
      borderRadius: 12,
      fontSize: '0.8rem',
      fontWeight: 600,
      border: `1px solid ${colors.color}33`,
    }}>
      {status}
    </span>
  );
}

export function EventListPage() {
  const navigate = useNavigate();
  const [events, setEvents] = useState<EventDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<EventStatus>('All');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');

  useEffect(() => {
    setLoading(true);
    setError(null);
    getEvents(statusFilter, 'startDate')
      .then((data) => {
        const sorted = [...data].sort((a, b) => {
          const diff = new Date(a.startDate).getTime() - new Date(b.startDate).getTime();
          return sortDir === 'asc' ? diff : -diff;
        });
        setEvents(sorted);
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [statusFilter, sortDir]);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '0.75rem' }}>
        <h1 className="page-title">Events</h1>
        <button
          onClick={() => navigate('/events/new')}
          className="btn-primary"
          style={{ padding: '8px 18px', borderRadius: 6 }}
        >
          + Create Event
        </button>
      </div>

      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', alignItems: 'center', flexWrap: 'wrap' }}>
        <label htmlFor="status-filter" style={{ fontWeight: 500 }}>Status:</label>
        <select
          id="status-filter"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as EventStatus)}
          style={{ padding: '4px 10px', borderRadius: 4, background: '#0d0d2b', color: '#e8e8ff', border: '1px solid #1e1e42' }}
        >
          {(['All', 'Draft', 'Published', 'Active', 'Closed'] as EventStatus[]).map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>

        <button
          onClick={() => setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'))}
          className="btn-ghost"
          style={{ padding: '4px 12px', borderRadius: 4 }}
        >
          Date {sortDir === 'asc' ? '↑' : '↓'}
        </button>
      </div>

      {loading && <p style={{ color: '#9ca3c8' }}>Loading events…</p>}
      {error && <p style={{ color: 'var(--danger)' }}>Error: {error}</p>}

      {!loading && !error && events.length === 0 && (
        <p style={{ color: '#5a5a80' }}>No events found.</p>
      )}

      {!loading && !error && events.length > 0 && (
        <div style={{ overflowX: 'auto', borderRadius: 8, border: '1px solid #1e1e42' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #1e1e42', background: '#0d0d2b', textAlign: 'left' }}>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Name</th>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Location</th>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Start Date</th>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Capacity</th>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Status</th>
            </tr>
          </thead>
          <tbody>
            {events.map((ev, idx) => (
              <tr
                key={ev.id}
                onClick={() => navigate(`/events/${ev.id}`)}
                style={{ borderBottom: '1px solid #1e1e42', cursor: 'pointer', background: idx % 2 === 0 ? 'transparent' : 'rgba(13,13,43,0.4)', transition: 'background 0.1s' }}
                onMouseEnter={(e) => (e.currentTarget.style.background = 'rgba(0,212,255,0.06)')}
                onMouseLeave={(e) => (e.currentTarget.style.background = idx % 2 === 0 ? 'transparent' : 'rgba(13,13,43,0.4)')}
              >
                <td style={{ padding: '10px 8px', fontWeight: 500, color: '#e8e8ff' }}>{ev.name}</td>
                <td style={{ padding: '10px 8px', color: '#9ca3c8' }}>{ev.location ?? '—'}</td>
                <td style={{ padding: '10px 8px', color: '#9ca3c8' }}>{new Date(ev.startDate).toLocaleString()}</td>
                <td style={{ padding: '10px 8px', color: '#9ca3c8' }}>{ev.capacity}</td>
                <td style={{ padding: '10px 8px' }}><StatusBadge status={ev.status} /></td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      )}
    </div>
  );
}
