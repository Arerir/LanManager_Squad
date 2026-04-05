import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { config } from '../config';
import { getAttendance } from '../api/attendance';
import type { AttendanceDto } from '../api/attendance';

interface CheckedInBroadcast {
  eventId: string;
  userId: string;
  userName: string;
  checkedInAt: string;
}

interface CheckedOutBroadcast {
  eventId: string;
  userId: string;
  userName: string;
  checkedOutAt: string;
}

export function AttendancePage() {
  const [searchParams] = useSearchParams();
  const eventId = searchParams.get('eventId');

  const [attendees, setAttendees] = useState<AttendanceDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hubStatus, setHubStatus] = useState<string>('Connecting…');

  useEffect(() => {
    if (!eventId) return;

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
      if (eventId && broadcast.eventId !== eventId) return;
      setAttendees(prev => {
        if (prev.some(a => a.userId === broadcast.userId)) return prev;
        return [...prev, {
          userId: broadcast.userId,
          userName: broadcast.userName,
          checkedInAt: broadcast.checkedInAt,
        }];
      });
    });

    connection.on('UserCheckedOut', (broadcast: CheckedOutBroadcast) => {
      if (eventId && broadcast.eventId !== eventId) return;
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

    return () => {
      connection.stop();
    };
  }, [eventId]);

  if (!eventId) {
    return (
      <div>
        <h1>Live Attendance</h1>
        <p style={{ color: '#aaa' }}>Select an event to view attendance.</p>
      </div>
    );
  }

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Live Attendance</h1>
        <span style={{
          background: hubStatus === 'Live' ? '#2a7a2a' : '#555',
          color: '#fff',
          borderRadius: '999px',
          padding: '2px 12px',
          fontSize: '0.8rem',
          fontWeight: 600,
        }}>
          {hubStatus}
        </span>
        <span style={{
          background: '#1a1a2e',
          color: '#7eb3ff',
          borderRadius: '999px',
          padding: '2px 12px',
          fontSize: '0.85rem',
          fontWeight: 600,
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
      )}
    </div>
  );
}

