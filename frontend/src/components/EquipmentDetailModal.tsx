import { useEffect, useRef } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { getUser } from '../api/auth';
import type { EquipmentDto } from '../api/equipment';

interface Props {
  equipment: EquipmentDto;
  onClose: () => void;
  onEdit: (id: string) => void;
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

function Field({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div style={{ marginBottom: '0.85rem' }}>
      <div style={labelStyle}>{label}</div>
      <div style={valueStyle}>{value ?? '—'}</div>
    </div>
  );
}

export function EquipmentDetailModal({ equipment, onClose, onEdit }: Props) {
  const dialogRef = useRef<HTMLDivElement>(null);
  const user = getUser();
  const canEdit = user?.roles.some(r => ['Admin', 'Organizer'].includes(r)) ?? false;

  // Focus trap + Escape key handler
  useEffect(() => {
    const prev = document.activeElement as HTMLElement | null;
    dialogRef.current?.focus();

    function handleKey(e: KeyboardEvent) {
      if (e.key === 'Escape') {
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
        if (document.activeElement === first) {
          e.preventDefault();
          last.focus();
        }
      } else {
        if (document.activeElement === last) {
          e.preventDefault();
          first.focus();
        }
      }
    }

    document.addEventListener('keydown', handleKey);
    return () => {
      document.removeEventListener('keydown', handleKey);
      prev?.focus();
    };
  }, [onClose]);

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
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            padding: '1.25rem 1.5rem 0',
          }}
        >
          <h2
            id="eq-modal-title"
            style={{ margin: 0, color: '#e8e8ff', fontSize: '1.2rem', fontWeight: 600 }}
          >
            {equipment.name}
          </h2>
          <button
            onClick={onClose}
            aria-label="Close modal"
            className="btn-ghost"
            style={{ padding: '4px 10px', fontSize: '1.1rem', lineHeight: 1 }}
          >
            ✕
          </button>
        </div>

        {/* QR Code */}
        <div
          style={{
            display: 'flex',
            justifyContent: 'center',
            padding: '1.5rem 1.5rem 1rem',
          }}
        >
          <div
            style={{
              background: '#fff',
              padding: 12,
              borderRadius: 8,
              display: 'inline-block',
              boxShadow: '0 0 0 1px rgba(0,212,255,0.2)',
            }}
          >
            <QRCodeSVG value={equipment.qrCode} size={220} />
          </div>
        </div>

        {/* Details */}
        <div style={{ padding: '0 1.5rem 1.25rem' }}>
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
              <Field
                label="Borrowed At"
                value={new Date(equipment.activeLoan.borrowedAt).toLocaleString()}
              />
              {equipment.activeLoan.returnedAt && (
                <Field
                  label="Returned At"
                  value={new Date(equipment.activeLoan.returnedAt).toLocaleString()}
                />
              )}
            </>
          )}
          {equipment.notes && <Field label="Notes" value={equipment.notes} />}
        </div>

        {/* Footer */}
        <div
          style={{
            display: 'flex',
            justifyContent: 'flex-end',
            gap: '0.75rem',
            padding: '1rem 1.5rem',
            borderTop: '1px solid #1e1e42',
          }}
        >
          <button onClick={onClose} className="btn-ghost">
            Close
          </button>
          {canEdit && (
            <button onClick={() => onEdit(equipment.id)} className="btn-primary">
              Edit
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
