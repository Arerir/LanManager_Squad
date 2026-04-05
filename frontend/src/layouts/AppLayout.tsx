import { Outlet, NavLink } from 'react-router-dom';

export function AppLayout() {
  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <nav style={{ width: 220, background: '#1a1a2e', color: '#fff', padding: '1rem' }}>
        <h2 style={{ marginBottom: '1.5rem' }}>LanManager</h2>
        <ul style={{ listStyle: 'none', padding: 0 }}>
          <li><NavLink to="/" style={{ color: '#fff' }}>Dashboard</NavLink></li>
          <li><NavLink to="/events" style={{ color: '#fff' }}>Events</NavLink></li>
          <li><NavLink to="/users" style={{ color: '#fff' }}>Users</NavLink></li>
          <li><NavLink to="/attendance" style={{ color: '#fff' }}>Attendance</NavLink></li>
        </ul>
      </nav>
      <main style={{ flex: 1, padding: '2rem' }}>
        <Outlet />
      </main>
    </div>
  );
}
