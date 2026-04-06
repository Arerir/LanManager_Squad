import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getEvents } from '../api/events';
import type { EventDto, EventStatus } from '../api/events';

const STATUS_COLORS: Record<string, string> = {
  Draft: '#6c757d',
  Published: '#0d6efd',
  Active: '#198754',
  Closed: '#dc3545',
};

function StatusBadge({ status }: { status: string }) {
  return (
    <span style={{
      background: STATUS_COLORS[status] ?? '#6c757d',
      color: '#fff',
      padding: '2px 10px',
      borderRadius: 12,
      fontSize: '0.8rem',
      fontWeight: 600,
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
          style={{ background: '#0d6efd', color: '#fff', border: 'none', padding: '8px 18px', borderRadius: 6, cursor: 'pointer', fontWeight: 600 }}
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
          style={{ padding: '4px 10px', borderRadius: 4 }}
        >
          {(['All', 'Draft', 'Published', 'Active', 'Closed'] as EventStatus[]).map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>

        <button
          onClick={() => setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'))}
          style={{ padding: '4px 12px', borderRadius: 4, cursor: 'pointer' }}
        >
          Date {sortDir === 'asc' ? '↑' : '↓'}
        </button>
      </div>

      {loading && <p>Loading events…</p>}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && !error && events.length === 0 && (
        <p style={{ color: '#888' }}>No events found.</p>
      )}

      {!loading && !error && events.length > 0 && (
        <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #dee2e6', textAlign: 'left' }}>
              <th style={{ padding: '10px 8px' }}>Name</th>
              <th style={{ padding: '10px 8px' }}>Location</th>
              <th style={{ padding: '10px 8px' }}>Start Date</th>
              <th style={{ padding: '10px 8px' }}>Capacity</th>
              <th style={{ padding: '10px 8px' }}>Status</th>
            </tr>
          </thead>
          <tbody>
            {events.map((ev) => (
              <tr
                key={ev.id}
                onClick={() => navigate(`/events/${ev.id}`)}
                style={{ borderBottom: '1px solid #dee2e6', cursor: 'pointer' }}
                onMouseEnter={(e) => (e.currentTarget.style.background = '#f8f9fa')}
                onMouseLeave={(e) => (e.currentTarget.style.background = '')}
              >
                <td style={{ padding: '10px 8px', fontWeight: 500 }}>{ev.name}</td>
                <td style={{ padding: '10px 8px', color: '#666' }}>{ev.location ?? '—'}</td>
                <td style={{ padding: '10px 8px' }}>{new Date(ev.startDate).toLocaleString()}</td>
                <td style={{ padding: '10px 8px' }}>{ev.capacity}</td>
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
