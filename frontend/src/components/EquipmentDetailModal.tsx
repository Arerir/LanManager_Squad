import { useEffect, useRef, useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { getUser } from '../api/auth';
import { updateEquipment, returnEquipment } from '../api/equipment';
import type { EquipmentDto } from '../api/equipment';

const TYPES = ['Computer', 'Keyboard', 'Mouse', 'Mousemat', 'VrHeadset', 'Other'];

interface Props {
  equipment: EquipmentDto;
  onClose: () => void;
  onSaved?: (updated: EquipmentDto) => void;
  onUnregistered?: () => void;
}

const labelStyle: React.CSSProperties = {
  fontSize: '0.8rem',
  fontWeight: 500,
  color: '#9ca3c8',
  textTransform: 'uppercase',
  letterSpacing: '0.06em',
  marginBottom: '0.2rem',
};

const valueStyle: React.CSSProperties = {
  color: '#e8e8ff',
  fontSize: '0.95rem',
  wordBreak: 'break-all',
};

const inputStyle: React.CSSProperties = {
  width: '100%',
  padding: '7px 10px',
  borderRadius: 4,
  border: '1px solid #1e1e42',
  fontSize: '0.95rem',
  boxSizing: 'border-box',
  background: '#060612',
  color: '#e8e8ff',
};

const errorStyle: React.CSSProperties = {
  color: 'var(--danger)',
  fontSize: '0.8rem',
  marginTop: '0.2rem',
};

function Field({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div style={{ marginBottom: '0.85rem' }}>
      <div style={labelStyle}>{label}</div>
      <div style={valueStyle}>{value ?? '—'}</div>
    </div>
  );
}

export function EquipmentDetailModal({ equipment, onClose, onSaved, onUnregistered }: Props) {
  const dialogRef = useRef<HTMLDivElement>(null);
  const user = getUser();
  const isElevated = user?.roles.some(r => ['Admin', 'Organizer', 'Operator'].includes(r)) ?? false;

  const [isEditing, setIsEditing] = useState(false);
  const [editName, setEditName] = useState(equipment.name);
  const [editType, setEditType] = useState(equipment.type);
  const [editNotes, setEditNotes] = useState(equipment.notes ?? '');
  const [nameError, setNameError] = useState('');
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState('');

  const [showUnregisterConfirm, setShowUnregisterConfirm] = useState(false);
  const [unregistering, setUnregistering] = useState(false);
  const [unregisterError, setUnregisterError] = useState('');

  function startEdit() {
    setEditName(equipment.name);
    setEditType(equipment.type);
    setEditNotes(equipment.notes ?? '');
    setNameError('');
    setSaveError('');
    setIsEditing(true);
  }

  function cancelEdit() {
    setIsEditing(false);
    setNameError('');
    setSaveError('');
  }

  async function handleSave() {
    if (!editName.trim()) {
      setNameError('Name is required');
      return;
    }
    setSaving(true);
    setSaveError('');
    try {
      const updated = await updateEquipment(equipment.id, {
        name: editName.trim(),
        type: editType,
        notes: editNotes.trim() || undefined,
      });
      setIsEditing(false);
      onSaved?.(updated);
    } catch (e) {
      setSaveError(e instanceof Error ? e.message : 'Save failed');
    } finally {
      setSaving(false);
    }
  }

  async function handleUnregister() {
    setUnregistering(true);
    setUnregisterError('');
    try {
      await returnEquipment(equipment.id);
      setShowUnregisterConfirm(false);
      onUnregistered?.();
    } catch (e) {
      setUnregisterError(e instanceof Error ? e.message : 'Unregister failed');
    } finally {
      setUnregistering(false);
    }
  }

  // Focus trap + Escape key handler
  useEffect(() => {
    const prev = document.activeElement as HTMLElement | null;
    dialogRef.current?.focus();

    function handleKey(e: KeyboardEvent) {
      if (e.key === 'Escape') {
        if (showUnregisterConfirm) { setShowUnregisterConfirm(false); return; }
        if (isEditing) { cancelEdit(); return; }
        onClose();
        return;
      }
      if (e.key !== 'Tab') return;

      const focusable = dialogRef.current?.querySelectorAll<HTMLElement>(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      );
      if (!focusable || focusable.length === 0) return;
      const first = focusable[0];
      const last = focusable[focusable.length - 1];

      if (e.shiftKey) {
        if (document.activeElement === first) { e.preventDefault(); last.focus(); }
      } else {
        if (document.activeElement === last) { e.preventDefault(); first.focus(); }
      }
    }

    document.addEventListener('keydown', handleKey);
    return () => {
      document.removeEventListener('keydown', handleKey);
      prev?.focus();
    };
  }, [onClose, isEditing, showUnregisterConfirm]);

  function handleBackdropClick(e: React.MouseEvent<HTMLDivElement>) {
    if (e.target === e.currentTarget) onClose();
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="eq-modal-title"
      onClick={handleBackdropClick}
      style={{
        position: 'fixed',
        inset: 0,
        background: 'rgba(0,0,0,0.75)',
        backdropFilter: 'blur(4px)',
        zIndex: 1000,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '1rem',
      }}
    >
      <div
        ref={dialogRef}
        tabIndex={-1}
        style={{
          background: '#0d0d2b',
          border: '1px solid #1e1e42',
          borderRadius: 8,
          width: '100%',
          maxWidth: 520,
          boxShadow: '0 8px 32px rgba(0,0,0,0.6), 0 0 0 1px rgba(0,212,255,0.1)',
          outline: 'none',
          maxHeight: '90vh',
          overflowY: 'auto',
        }}
      >
        {/* Header */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '1.25rem 1.5rem 0' }}>
          <h2 id="eq-modal-title" style={{ margin: 0, color: '#e8e8ff', fontSize: '1.2rem', fontWeight: 600 }}>
            {isEditing ? 'Edit Equipment' : equipment.name}
          </h2>
          <button onClick={onClose} aria-label="Close modal" className="btn-ghost" style={{ padding: '4px 10px', fontSize: '1.1rem', lineHeight: 1 }}>
            ✕
          </button>
        </div>

        {/* QR Code — never editable */}
        <div style={{ display: 'flex', justifyContent: 'center', padding: '1.5rem 1.5rem 1rem' }}>
          <div style={{ background: '#fff', padding: 12, borderRadius: 8, display: 'inline-block', boxShadow: '0 0 0 1px rgba(0,212,255,0.2)' }}>
            <QRCodeSVG value={equipment.qrCode} size={220} />
          </div>
        </div>

        {/* Details / Edit form */}
        <div style={{ padding: '0 1.5rem 1.25rem' }}>
          {saveError && <p style={{ color: 'var(--danger)', fontSize: '0.85rem', marginBottom: '0.75rem' }}>{saveError}</p>}

          {isEditing ? (
            <>
              <div style={{ marginBottom: '0.85rem' }}>
                <label style={labelStyle} htmlFor="eq-edit-name">Name *</label>
                <input
                  id="eq-edit-name"
                  value={editName}
                  onChange={e => { setEditName(e.target.value); if (e.target.value.trim()) setNameError(''); }}
                  style={{ ...inputStyle, borderColor: nameError ? 'var(--danger)' : '#1e1e42' }}
                />
                {nameError && <div style={errorStyle}>{nameError}</div>}
              </div>

              <div style={{ marginBottom: '0.85rem' }}>
                <label style={labelStyle} htmlFor="eq-edit-type">Type</label>
                <select id="eq-edit-type" value={editType} onChange={e => setEditType(e.target.value)} style={inputStyle}>
                  {TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              </div>

              <Field label="QR Code" value={<code style={{ fontSize: '0.85rem' }}>{equipment.qrCode}</code>} />

              <Field
                label="Status"
                value={
                  <span className={equipment.isAvailable ? 'badge-available' : 'badge-loan'}>
                    {equipment.isAvailable ? 'Available' : 'On Loan'}
                  </span>
                }
              />

              {equipment.activeLoan && (
                <>
                  <Field label="Borrowed By" value={equipment.activeLoan.userName} />
                  <Field label="Borrowed At" value={new Date(equipment.activeLoan.borrowedAt).toLocaleString()} />
                </>
              )}

              <div style={{ marginBottom: '0.85rem' }}>
                <label style={labelStyle} htmlFor="eq-edit-notes">Notes</label>
                <textarea
                  id="eq-edit-notes"
                  value={editNotes}
                  onChange={e => setEditNotes(e.target.value)}
                  style={{ ...inputStyle, resize: 'vertical', minHeight: 70, fontFamily: 'var(--sans)' }}
                  placeholder="Optional notes…"
                />
              </div>
            </>
          ) : (
            <>
              <Field label="QR Code" value={<code style={{ fontSize: '0.85rem' }}>{equipment.qrCode}</code>} />
              <Field label="Type" value={equipment.type} />
              <Field
                label="Status"
                value={
                  <span className={equipment.isAvailable ? 'badge-available' : 'badge-loan'}>
                    {equipment.isAvailable ? 'Available' : 'On Loan'}
                  </span>
                }
              />
              {equipment.activeLoan && (
                <>
                  <Field label="Borrowed By" value={equipment.activeLoan.userName} />
                  <Field label="Borrowed At" value={new Date(equipment.activeLoan.borrowedAt).toLocaleString()} />
                  {equipment.activeLoan.returnedAt && (
                    <Field label="Returned At" value={new Date(equipment.activeLoan.returnedAt).toLocaleString()} />
                  )}
                </>
              )}
              {equipment.notes && <Field label="Notes" value={equipment.notes} />}
            </>
          )}

          {/* Unregister confirmation inline */}
          {showUnregisterConfirm && (
            <div style={{ marginTop: '0.75rem', padding: '0.85rem 1rem', background: 'rgba(220,38,38,0.12)', border: '1px solid rgba(220,38,38,0.35)', borderRadius: 6 }}>
              <p style={{ margin: '0 0 0.6rem', color: '#e8e8ff', fontSize: '0.9rem' }}>
                Unregister <strong>{equipment.activeLoan?.userName}</strong> from this equipment?
              </p>
              {unregisterError && <p style={{ color: 'var(--danger)', fontSize: '0.82rem', margin: '0 0 0.5rem' }}>{unregisterError}</p>}
              <div style={{ display: 'flex', gap: '0.6rem' }}>
                <button onClick={handleUnregister} disabled={unregistering} className="btn-danger" style={{ fontSize: '0.85rem' }}>
                  {unregistering ? 'Unregistering…' : 'Confirm Unregister'}
                </button>
                <button onClick={() => { setShowUnregisterConfirm(false); setUnregisterError(''); }} className="btn-ghost" style={{ fontSize: '0.85rem' }}>
                  Cancel
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '0.75rem', padding: '1rem 1.5rem', borderTop: '1px solid #1e1e42', flexWrap: 'wrap' }}>
          <div>
            {isElevated && !equipment.isAvailable && !isEditing && !showUnregisterConfirm && (
              <button onClick={() => setShowUnregisterConfirm(true)} className="btn-danger" style={{ fontSize: '0.85rem' }}>
                Unregister
              </button>
            )}
          </div>
          <div style={{ display: 'flex', gap: '0.75rem' }}>
            {isEditing ? (
              <>
                <button onClick={cancelEdit} className="btn-ghost" disabled={saving}>Cancel</button>
                <button onClick={handleSave} className="btn-primary" disabled={saving}>
                  {saving ? 'Saving…' : 'Save'}
                </button>
              </>
            ) : (
              <>
                <button onClick={onClose} className="btn-ghost">Close</button>
                {isElevated && !showUnregisterConfirm && (
                  <button onClick={startEdit} className="btn-primary">Edit</button>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

