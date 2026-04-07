import { useState, useEffect } from 'react';
import { getUser } from '../api/auth';
import { getEquipment, createEquipment, returnEquipment } from '../api/equipment';
import type { EquipmentDto } from '../api/equipment';

const TYPES = ['Computer', 'Keyboard', 'Mouse', 'Mousemat', 'VrHeadset', 'Other'];

const thStyle: React.CSSProperties = { padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' };
const tdStyle: React.CSSProperties = { padding: '10px 8px', color: '#e8e8ff' };

const selectStyle: React.CSSProperties = {
  padding: '6px 10px',
  borderRadius: 4,
  border: '1px solid #1e1e42',
  fontSize: '0.9rem',
  background: '#0d0d2b',
  color: '#e8e8ff',
};

export function EquipmentPage() {
  const [equipment, setEquipment] = useState<EquipmentDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [typeFilter, setTypeFilter] = useState('');
  const [availFilter, setAvailFilter] = useState('');
  const [showAdd, setShowAdd] = useState(false);

  const user = getUser();
  const canManage = user?.roles.some(r => ['Admin', 'Organizer', 'Operator'].includes(r));
  const canAdd = user?.roles.includes('Admin');

  useEffect(() => { loadEquipment(); }, []);

  async function loadEquipment() {
    setLoading(true);
    try {
      setEquipment(await getEquipment());
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Load failed');
    } finally {
      setLoading(false);
    }
  }

  async function handleReturn(id: string) {
    try {
      await returnEquipment(id);
      await loadEquipment();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Return failed');
    }
  }

  const filtered = equipment
    .filter(e => !typeFilter || e.type === typeFilter)
    .filter(e =>
      availFilter === 'available' ? e.isAvailable
        : availFilter === 'loan' ? !e.isAvailable
        : true
    );

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '0.75rem' }}>
        <h1 className="page-title">Equipment Inventory</h1>
        {canAdd && (
          <button
            onClick={() => setShowAdd(true)}
            className="btn-primary"
          >
            + Add Equipment
          </button>
        )}
      </div>

      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        <select value={typeFilter} onChange={e => setTypeFilter(e.target.value)} style={selectStyle}>
          <option value="">All Types</option>
          {TYPES.map(t => <option key={t} value={t}>{t}</option>)}
        </select>
        <select value={availFilter} onChange={e => setAvailFilter(e.target.value)} style={selectStyle}>
          <option value="">All Status</option>
          <option value="available">Available</option>
          <option value="loan">On Loan</option>
        </select>
      </div>

      {error && <p style={{ color: 'var(--danger)', marginBottom: '1rem' }}>{error}</p>}
      {loading && <p style={{ color: 'var(--text-muted)' }}>Loading…</p>}

      {!loading && (
        <div style={{ overflowX: 'auto', borderRadius: 8, border: '1px solid #1e1e42' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid #1e1e42', background: '#0d0d2b', textAlign: 'left' }}>
                <th style={thStyle}>Name</th>
                <th style={thStyle}>Type</th>
                <th style={thStyle}>QR Code</th>
                <th style={thStyle}>Status</th>
                <th style={thStyle}>Borrowed By</th>
                <th style={thStyle}>Borrowed At</th>
                {canManage && <th style={thStyle}>Action</th>}
              </tr>
            </thead>
            <tbody>
              {filtered.length === 0 && (
                <tr>
                  <td colSpan={canManage ? 7 : 6} style={{ ...tdStyle, color: '#5a5a80', textAlign: 'center', padding: '2rem' }}>
                    No equipment found.
                  </td>
                </tr>
              )}
              {filtered.map((item, idx) => (
                <tr key={item.id} style={{ borderBottom: '1px solid #1e1e42', background: idx % 2 === 0 ? 'transparent' : 'rgba(13,13,43,0.4)' }}>
                  <td style={tdStyle}>{item.name}</td>
                  <td style={{ ...tdStyle, color: '#9ca3c8' }}>{item.type}</td>
                  <td style={tdStyle}><code style={{ fontSize: '0.85rem' }}>{item.qrCode}</code></td>
                  <td style={tdStyle}>
                    <span className={item.isAvailable ? 'badge-available' : 'badge-loan'}>
                      {item.isAvailable ? 'Available' : 'On Loan'}
                    </span>
                  </td>
                  <td style={{ ...tdStyle, color: '#9ca3c8' }}>{item.activeLoan?.userName ?? '—'}</td>
                  <td style={{ ...tdStyle, color: '#9ca3c8' }}>
                    {item.activeLoan ? new Date(item.activeLoan.borrowedAt).toLocaleString() : '—'}
                  </td>
                  {canManage && (
                    <td style={tdStyle}>
                      {!item.isAvailable && (
                        <button onClick={() => handleReturn(item.id)} className="btn-danger">
                          Return
                        </button>
                      )}
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showAdd && (
        <AddEquipmentModal
          onClose={() => setShowAdd(false)}
          onAdded={() => { setShowAdd(false); loadEquipment(); }}
        />
      )}
    </div>
  );
}

function AddEquipmentModal({ onClose, onAdded }: { onClose: () => void; onAdded: () => void }) {
  const [name, setName] = useState('');
  const [type, setType] = useState('Computer');
  const [qrCode, setQrCode] = useState('');
  const [notes, setNotes] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!name.trim() || !qrCode.trim()) { setError('Name and QR Code are required'); return; }
    setSaving(true);
    try {
      await createEquipment({ name: name.trim(), type, qrCode: qrCode.trim(), notes: notes.trim() || undefined });
      onAdded();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create equipment');
    } finally {
      setSaving(false);
    }
  }

  const inputStyle: React.CSSProperties = {
    width: '100%', padding: '8px 10px', borderRadius: 4,
    border: '1px solid #1e1e42', fontSize: '0.95rem', boxSizing: 'border-box',
    background: '#060612', color: '#e8e8ff',
  };
  const labelStyle: React.CSSProperties = { display: 'block', marginBottom: '0.25rem', fontWeight: 500, fontSize: '0.9rem', color: '#9ca3c8' };
  const fieldStyle: React.CSSProperties = { marginBottom: '1rem' };

  return (
    <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.75)', backdropFilter: 'blur(4px)', zIndex: 1000, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <div style={{
        background: '#0d0d2b',
        border: '1px solid #1e1e42',
        borderRadius: 8,
        padding: '2rem',
        width: '100%',
        maxWidth: 420,
        boxShadow: '0 8px 32px rgba(0,0,0,0.6), 0 0 0 1px rgba(0,212,255,0.1)',
      }}>
        <h2 style={{ margin: '0 0 1.5rem', color: '#e8e8ff' }}>Add Equipment</h2>
        {error && <p style={{ color: 'var(--danger)', marginBottom: '1rem', fontSize: '0.9rem' }}>{error}</p>}
        <form onSubmit={handleSubmit}>
          <div style={fieldStyle}>
            <label style={labelStyle}>Name *</label>
            <input value={name} onChange={e => setName(e.target.value)} style={inputStyle} placeholder="e.g. Gaming PC #3" />
          </div>
          <div style={fieldStyle}>
            <label style={labelStyle}>Type *</label>
            <select value={type} onChange={e => setType(e.target.value)} style={{ ...inputStyle }}>
              {TYPES.map(t => <option key={t} value={t}>{t}</option>)}
            </select>
          </div>
          <div style={fieldStyle}>
            <label style={labelStyle}>QR Code *</label>
            <input value={qrCode} onChange={e => setQrCode(e.target.value)} style={inputStyle} placeholder="e.g. EQ-001" />
          </div>
          <div style={fieldStyle}>
            <label style={labelStyle}>Notes</label>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} style={{ ...inputStyle, resize: 'vertical', minHeight: 70, fontFamily: 'var(--sans)' }} placeholder="Optional notes…" />
          </div>
          <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
            <button type="button" onClick={onClose} className="btn-ghost">
              Cancel
            </button>
            <button type="submit" disabled={saving} className="btn-primary">
              {saving ? 'Saving…' : 'Add Equipment'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
