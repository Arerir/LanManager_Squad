import { Link, useNavigate } from 'react-router-dom';
import type { UserDto } from '../api/users';

export function ProfilePage() {
  const navigate = useNavigate();
  const raw = localStorage.getItem('currentUser');
  const user: UserDto | null = raw ? (JSON.parse(raw) as UserDto) : null;

  function handleLogout() {
    localStorage.removeItem('currentUser');
    navigate('/login');
  }

  if (!user) {
    return (
      <div style={{ maxWidth: 480 }}>
        <h1>Profile</h1>
        <p style={{ color: '#555' }}>You are not logged in.</p>
        <Link to="/register" style={{ color: '#0d6efd' }}>Create an account</Link>
        {' or '}
        <Link to="/login" style={{ color: '#0d6efd' }}>sign in</Link>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Profile</h1>
        <button
          onClick={handleLogout}
          style={{ background: '#6c757d', color: '#fff', border: 'none', padding: '8px 16px', borderRadius: 6, cursor: 'pointer' }}
        >
          Sign Out
        </button>
      </div>

      <div style={{ display: 'grid', gap: '0.75rem' }}>
        <ProfileField label="Name">{user.name}</ProfileField>
        <ProfileField label="Username">@{user.userName}</ProfileField>
        <ProfileField label="Email">{user.email}</ProfileField>
      </div>
    </div>
  );
}

function ProfileField({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', gap: '1rem', alignItems: 'baseline', padding: '10px 0', borderBottom: '1px solid #dee2e6' }}>
      <span style={{ fontWeight: 600, minWidth: 100, color: '#555' }}>{label}</span>
      <span>{children}</span>
    </div>
  );
}
