import { useState, useEffect } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { clearToken, getUser } from '../api/auth';

const navLinkStyle = ({ isActive }: { isActive: boolean }): React.CSSProperties => ({
  color: isActive ? '#00d4ff' : '#9ca3c8',
  textDecoration: 'none',
  display: 'block',
  padding: '6px 10px',
  borderRadius: 4,
  fontWeight: isActive ? 600 : 400,
  background: isActive ? 'rgba(0, 212, 255, 0.1)' : 'transparent',
  borderLeft: isActive ? '2px solid #00d4ff' : '2px solid transparent',
  transition: 'color 0.15s, background 0.15s',
});

export function AppLayout() {
  const navigate = useNavigate();
  const currentUser = getUser();
  const [navOpen, setNavOpen] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);

  useEffect(() => {
    const handle = () => {
      const mobile = window.innerWidth <= 768;
      setIsMobile(mobile);
      if (!mobile) setNavOpen(false);
    };
    window.addEventListener('resize', handle);
    return () => window.removeEventListener('resize', handle);
  }, []);

  function handleLogout() {
    clearToken();
    navigate('/login');
  }

  const sidebarStyle: React.CSSProperties = {
    width: 220,
    background: '#080818',
    borderRight: '1px solid #1e1e42',
    color: '#e8e8ff',
    padding: '1rem',
    display: isMobile ? (navOpen ? 'flex' : 'none') : 'flex',
    flexDirection: 'column',
    position: isMobile ? 'fixed' : 'relative',
    zIndex: isMobile ? 1000 : 'auto',
    height: isMobile ? '100vh' : 'auto',
    top: 0,
    left: 0,
    overflowY: 'auto',
  };

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      {/* Mobile overlay */}
      {isMobile && navOpen && (
        <div
          onClick={() => setNavOpen(false)}
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
            zIndex: 999,
          }}
        />
      )}

      <nav style={sidebarStyle}>
        <h2 style={{ marginBottom: '1.5rem', color: '#00d4ff', letterSpacing: '-0.5px', textShadow: '0 0 12px rgba(0,212,255,0.4)' }}>LanManager</h2>
        <ul style={{ listStyle: 'none', padding: 0, flex: 1, margin: 0 }}>
          <li><NavLink to="/" end style={navLinkStyle} onClick={() => setNavOpen(false)}>Dashboard</NavLink></li>
          <li><NavLink to="/events" style={navLinkStyle} onClick={() => setNavOpen(false)}>Events</NavLink></li>
          <li><NavLink to="/users" style={navLinkStyle} onClick={() => setNavOpen(false)}>Users</NavLink></li>
          <li><NavLink to="/attendance" style={navLinkStyle} onClick={() => setNavOpen(false)}>Attendance</NavLink></li>
          <li><NavLink to="/tournaments" style={navLinkStyle} onClick={() => setNavOpen(false)}>Tournaments</NavLink></li>
          <li><NavLink to="/seating" style={navLinkStyle} onClick={() => setNavOpen(false)}>Seating</NavLink></li>
          <li><NavLink to="/equipment" style={navLinkStyle} onClick={() => setNavOpen(false)}>Equipment</NavLink></li>
        </ul>
        <div style={{ borderTop: '1px solid #1e1e42', paddingTop: '1rem', marginTop: 'auto' }}>
          {currentUser ? (
            <>
              <NavLink to="/profile" style={navLinkStyle} onClick={() => setNavOpen(false)}>
                {currentUser.name}
              </NavLink>
              <button
                onClick={handleLogout}
                style={{ background: 'none', border: 'none', color: '#5a5a80', cursor: 'pointer', padding: '4px 10px', fontSize: '0.85rem', textAlign: 'left', width: '100%' }}
              >
                Sign out
              </button>
            </>
          ) : (
            <>
              <NavLink to="/register" style={navLinkStyle} onClick={() => setNavOpen(false)}>Register</NavLink>
              <NavLink to="/login" style={navLinkStyle} onClick={() => setNavOpen(false)}>Sign In</NavLink>
            </>
          )}
        </div>
      </nav>

      <main style={{ flex: 1, padding: '2rem', textAlign: 'left', minWidth: 0, background: '#060612' }}>
        {isMobile && (
          <button
            onClick={() => setNavOpen(v => !v)}
            aria-label="Toggle navigation"
            style={{
              background: 'none', border: '1px solid #1e1e42', color: '#9ca3c8',
              fontSize: '1.2rem', cursor: 'pointer', padding: '4px 10px',
              borderRadius: 4, marginBottom: '1rem', display: 'block',
            }}
          >
            ☰
          </button>
        )}
        <Outlet />
      </main>
    </div>
  );
}