import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getEvent, createEvent, updateEvent } from '../api/events';
import type { CreateEventRequest, EventDto } from '../api/events';

type EventStatus = 'Draft' | 'Published' | 'Active' | 'Closed';

interface FormState {
  name: string;
  description: string;
  location: string;
  startDate: string;
  endDate: string;
  capacity: string;
  status: EventStatus;
}

const EMPTY_FORM: FormState = {
  name: '',
  description: '',
  location: '',
  startDate: '',
  endDate: '',
  capacity: '1',
  status: 'Draft',
};

function toFormState(ev: EventDto): FormState {
  const fmt = (iso: string) => iso.slice(0, 16);
  return {
    name: ev.name,
    description: ev.description ?? '',
    location: ev.location ?? '',
    startDate: fmt(ev.startDate),
    endDate: fmt(ev.endDate),
    capacity: String(ev.capacity),
    status: ev.status,
  };
}

interface ValidationErrors {
  name?: string;
  capacity?: string;
  endDate?: string;
}

function validate(form: FormState): ValidationErrors {
  const errors: ValidationErrors = {};
  if (!form.name.trim()) errors.name = 'Name is required.';
  const cap = Number(form.capacity);
  if (!Number.isInteger(cap) || cap < 1) errors.capacity = 'Capacity must be a positive integer.';
  if (form.startDate && form.endDate && form.endDate <= form.startDate) {
    errors.endDate = 'End date must be after start date.';
  }
  return errors;
}

const inputStyle: React.CSSProperties = {
  width: '100%',
  padding: '8px 10px',
  borderRadius: 4,
  border: '1px solid #ced4da',
  fontSize: '1rem',
  boxSizing: 'border-box',
};

function FormField({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
      <label style={{ fontWeight: 500, fontSize: '0.9rem' }}>{label}</label>
      {children}
      {error && <span style={{ color: '#dc3545', fontSize: '0.82rem' }}>{error}</span>}
    </div>
  );
}

export function EventFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [apiError, setApiError] = useState<string | null>(null);
  const [loading, setLoading] = useState(isEdit);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    getEvent(id)
      .then((ev) => setForm(toFormState(ev)))
      .catch((e: Error) => setApiError(e.message))
      .finally(() => setLoading(false));
  }, [id]);

  function handleChange(e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const validationErrors = validate(form);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    const payload: CreateEventRequest = {
      name: form.name.trim(),
      description: form.description.trim() || undefined,
      location: form.location.trim() || undefined,
      startDate: new Date(form.startDate).toISOString(),
      endDate: new Date(form.endDate).toISOString(),
      capacity: Number(form.capacity),
      status: form.status,
    };

    setSubmitting(true);
    setApiError(null);
    try {
      const result = isEdit && id
        ? await updateEvent(id, payload)
        : await createEvent(payload);
      navigate(`/events/${result.id}`);
    } catch (e) {
      setApiError((e as Error).message);
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) return <p>Loading…</p>;

  return (
    <div style={{ maxWidth: 560 }}>
      <button
        onClick={() => navigate(isEdit && id ? `/events/${id}` : '/events')}
        style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#0d6efd', padding: 0, marginBottom: 12, fontSize: '0.9rem' }}
      >
        ← Back
      </button>
      <h1 style={{ marginBottom: '1.5rem' }}>{isEdit ? 'Edit Event' : 'Create Event'}</h1>

      {apiError && (
        <div style={{ background: '#f8d7da', color: '#842029', padding: '10px 14px', borderRadius: 6, marginBottom: '1rem' }}>
          {apiError}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <FormField label="Name *" error={errors.name}>
          <input name="name" value={form.name} onChange={handleChange} required style={inputStyle} />
        </FormField>

        <FormField label="Description">
          <textarea name="description" value={form.description} onChange={handleChange} rows={3} style={{ ...inputStyle, resize: 'vertical' }} />
        </FormField>

        <FormField label="Location">
          <input name="location" value={form.location} onChange={handleChange} style={inputStyle} />
        </FormField>

        <FormField label="Start Date *">
          <input type="datetime-local" name="startDate" value={form.startDate} onChange={handleChange} required style={inputStyle} />
        </FormField>

        <FormField label="End Date *" error={errors.endDate}>
          <input type="datetime-local" name="endDate" value={form.endDate} onChange={handleChange} required style={inputStyle} />
        </FormField>

        <FormField label="Capacity *" error={errors.capacity}>
          <input type="number" name="capacity" value={form.capacity} onChange={handleChange} min={1} required style={inputStyle} />
        </FormField>

        <FormField label="Status">
          <select name="status" value={form.status} onChange={handleChange} style={inputStyle}>
            {(['Draft', 'Published', 'Active', 'Closed'] as EventStatus[]).map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </FormField>

        <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
          <button
            type="submit"
            disabled={submitting}
            style={{ background: '#0d6efd', color: '#fff', border: 'none', padding: '9px 22px', borderRadius: 6, cursor: 'pointer', fontWeight: 600 }}
          >
            {submitting ? 'Saving…' : isEdit ? 'Update Event' : 'Create Event'}
          </button>
          <button
            type="button"
            onClick={() => navigate(-1)}
            style={{ background: '#6c757d', color: '#fff', border: 'none', padding: '9px 22px', borderRadius: 6, cursor: 'pointer' }}
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
