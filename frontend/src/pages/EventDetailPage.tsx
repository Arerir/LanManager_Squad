import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getEvent, getEventAttendees, deleteEvent } from '../api/events';
import type { EventDto } from '../api/events';
import type { UserDto } from '../api/users';

const STATUS_COLORS: Record<string, string> = {
  Draft: '#6c757d',
  Published: '#0d6efd',
  Active: '#198754',
  Closed: '#dc3545',
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
    <div style={{ maxWidth: 700 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1.5rem' }}>
        <div>
          <button
            onClick={() => navigate('/events')}
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#0d6efd', padding: 0, marginBottom: 8, fontSize: '0.9rem' }}
          >
            ← Back to Events
          </button>
          <h1 style={{ margin: 0 }}>{event.name}</h1>
        </div>
        <div style={{ display: 'flex', gap: 8, marginTop: 32 }}>
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
