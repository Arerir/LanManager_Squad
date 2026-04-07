import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getEvent, getEventAttendees, deleteEvent } from '../api/events';
import type { EventDto } from '../api/events';
import type { UserDto } from '../api/users';

const STATUS_COLORS: Record<string, { bg: string; color: string }> = {
  Draft:     { bg: 'rgba(90,90,128,0.2)',   color: '#9ca3c8' },
  Published: { bg: 'rgba(0,212,255,0.15)',  color: '#00d4ff' },
  Active:    { bg: 'rgba(0,230,118,0.15)',  color: '#00e676' },
  Closed:    { bg: 'rgba(255,56,96,0.15)',  color: '#ff3860' },
};

export function EventDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [event, setEvent] = useState<EventDto | null>(null);
  const [attendees, setAttendees] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    setError(null);
    Promise.all([getEvent(id), getEventAttendees(id)])
      .then(([ev, att]) => {
        setEvent(ev);
        setAttendees(att);
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [id]);

  async function handleDelete() {
    if (!id || !window.confirm('Delete this event?')) return;
    setDeleting(true);
    try {
      await deleteEvent(id);
      navigate('/events');
    } catch (e) {
      setError((e as Error).message);
      setDeleting(false);
    }
  }

  if (loading) return <p>Loading…</p>;
  if (error) return <p style={{ color: 'red' }}>Error: {error}</p>;
  if (!event) return <p>Event not found.</p>;

  return (
    <div style={{ maxWidth: 700, width: '100%' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1.5rem' }}>
        <div>
          <button
            onClick={() => navigate('/events')}
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#0d6efd', padding: 0, marginBottom: 8, fontSize: '0.9rem' }}
          >
            ← Back to Events
          </button>
          <h1 className="page-title">{event.name}</h1>
        </div>
        <div style={{ display: 'flex', gap: 8, marginTop: 32, flexWrap: 'wrap' }}>
          <button
            onClick={() => navigate(`/events/${event.id}/edit`)}
            style={{ background: '#0d6efd', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 6, cursor: 'pointer' }}
          >
            Edit
          </button>
          <button
            onClick={() => navigate(`/attendance?eventId=${event.id}&tab=doorlog`)}
            style={{ background: '#6c757d', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 6, cursor: 'pointer' }}
          >
            📋 Door Log
          </button>
          <button onClick={() => navigate(`/seating?eventId=${event.id}`)}
            style={{ background: '#16a085', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 6, cursor: 'pointer' }}>
            🪑 Seating
          </button>
          <button onClick={() => navigate(`/tournaments?eventId=${event.id}`)}
            style={{ background: '#8e44ad', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 6, cursor: 'pointer' }}>
            🏆 Tournaments
          </button>
          <button
            onClick={handleDelete}
            disabled={deleting}
            style={{ background: '#dc3545', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 6, cursor: 'pointer' }}
          >
            {deleting ? 'Deleting…' : 'Delete'}
          </button>
        </div>
      </div>

      <div style={{ display: 'grid', gap: '0.75rem', marginBottom: '2rem' }}>
        <Field label="Status">
          <span style={{
            background: STATUS_COLORS[event.status] ?? '#6c757d',
            color: '#fff', padding: '2px 10px', borderRadius: 12, fontSize: '0.85rem', fontWeight: 600,
          }}>
            {event.status}
          </span>
        </Field>
        {event.description && <Field label="Description">{event.description}</Field>}
        <Field label="Location">{event.location ?? '—'}</Field>
        <Field label="Start">{new Date(event.startDate).toLocaleString()}</Field>
        <Field label="End">{new Date(event.endDate).toLocaleString()}</Field>
        <Field label="Capacity">{event.capacity}</Field>
        <Field label="Attendees">{attendees.length} / {event.capacity}</Field>
        <Field label="Created">{new Date(event.createdAt).toLocaleString()}</Field>
      </div>
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', gap: '1rem', alignItems: 'baseline' }}>
      <span style={{ fontWeight: 600, minWidth: 100, color: '#555' }}>{label}</span>
      <span>{children}</span>
    </div>
  );
}
