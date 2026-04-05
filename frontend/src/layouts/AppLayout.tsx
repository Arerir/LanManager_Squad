import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { clearToken, getUser } from '../api/auth';

const navLinkStyle = ({ isActive }: { isActive: boolean }): React.CSSProperties => ({
  color: isActive ? '#7eb3ff' : '#ccc',
  textDecoration: 'none',
  display: 'block',
  padding: '6px 0',
  fontWeight: isActive ? 600 : 400,
});

export function AppLayout() {
  const navigate = useNavigate();
  const currentUser = getUser();

  function handleLogout() {
    clearToken();
    navigate('/login');
  }

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <nav style={{ width: 220, background: '#1a1a2e', color: '#fff', padding: '1rem', display: 'flex', flexDirection: 'column' }}>
        <h2 style={{ marginBottom: '1.5rem', color: '#fff' }}>LanManager</h2>
        <ul style={{ listStyle: 'none', padding: 0, flex: 1 }}>
          <li><NavLink to="/" end style={navLinkStyle}>Dashboard</NavLink></li>
          <li><NavLink to="/events" style={navLinkStyle}>Events</NavLink></li>
          <li><NavLink to="/users" style={navLinkStyle}>Users</NavLink></li>
          <li><NavLink to="/attendance" style={navLinkStyle}>Attendance</NavLink></li>
        </ul>
        <div style={{ borderTop: '1px solid #333', paddingTop: '1rem', marginTop: 'auto' }}>
          {currentUser ? (
            <>
              <NavLink to="/profile" style={navLinkStyle}>
                {currentUser.name}
              </NavLink>
              <button
                onClick={handleLogout}
                style={{ background: 'none', border: 'none', color: '#aaa', cursor: 'pointer', padding: '4px 0', fontSize: '0.85rem', textAlign: 'left' }}
              >
                Sign out
              </button>
            </>
          ) : (
            <>
              <NavLink to="/register" style={navLinkStyle}>Register</NavLink>
              <NavLink to="/login" style={navLinkStyle}>Sign In</NavLink>
            </>
          )}
        </div>
      </nav>
      <main style={{ flex: 1, padding: '2rem' }}>
        <Outlet />
      </main>
    </div>
  );
}