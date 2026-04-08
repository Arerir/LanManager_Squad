import { useState } from 'react';
import { downloadEventReport } from '../api/events';

const ALL_SECTIONS = ['Registrations', 'CheckIns', 'Equipment', 'Tournaments'] as const;
type Section = typeof ALL_SECTIONS[number];

interface ReportDownloadButtonProps {
  eventId: string;
  eventStatus: string;
  userRole: string;
}

export function ReportDownloadButton({ eventId, eventStatus, userRole }: ReportDownloadButtonProps) {
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<Set<Section>>(new Set(ALL_SECTIONS));
  const [downloading, setDownloading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (eventStatus !== 'Closed') return null;
  if (userRole !== 'Admin' && userRole !== 'Organizer') return null;

  function toggle(section: Section) {
    setSelected(prev => {
      const next = new Set(prev);
      if (next.has(section)) {
        next.delete(section);
      } else {
        next.add(section);
      }
      return next;
    });
  }

  async function handleDownload() {
    setError(null);
    setDownloading(true);
    try {
      const sections = selected.size === ALL_SECTIONS.length
        ? ['All']
        : Array.from(selected);
      await downloadEventReport(eventId, sections);
      setOpen(false);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setDownloading(false);
    }
  }

  return (
    <div>
      <button
        onClick={() => setOpen(v => !v)}
        className="btn-primary"
        style={{ padding: '8px 16px', borderRadius: 6 }}
      >
        📄 Download Report
      </button>

      {open && (
        <div style={{
          marginTop: 8,
          padding: '1rem',
          background: 'var(--surface)',
          border: '1px solid rgba(0,212,255,0.2)',
          borderRadius: 8,
          display: 'flex',
          flexDirection: 'column',
          gap: '0.5rem',
          minWidth: 220,
        }}>
          <span style={{ color: 'var(--text)', fontWeight: 600, marginBottom: 4 }}>Sections</span>
          {ALL_SECTIONS.map(section => (
            <label
              key={section}
              style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', color: 'var(--text)' }}
            >
              <input
                type="checkbox"
                checked={selected.has(section)}
                onChange={() => toggle(section)}
                style={{ accentColor: 'var(--cyan)' }}
              />
              {section}
            </label>
          ))}

          {error && (
            <span style={{ color: 'var(--danger)', fontSize: '0.85rem', marginTop: 4 }}>
              {error}
            </span>
          )}

          <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
            <button
              onClick={handleDownload}
              disabled={downloading || selected.size === 0}
              className="btn-primary"
              style={{ padding: '6px 14px', borderRadius: 6, flex: 1 }}
            >
              {downloading ? '⏳ Downloading…' : 'Download'}
            </button>
            <button
              onClick={() => { setOpen(false); setError(null); }}
              className="btn-ghost"
              style={{ padding: '6px 14px', borderRadius: 6 }}
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
