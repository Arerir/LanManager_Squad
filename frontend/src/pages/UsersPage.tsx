import { useState, useEffect } from 'react';
import { getUsers } from '../api/users';
import type { UserDto } from '../api/users';

const PAGE_SIZE = 20;

export function UsersPage() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);

  useEffect(() => {
    setLoading(true);
    setError(null);
    getUsers(page, PAGE_SIZE)
      .then((data) => {
        setUsers(data);
        setHasMore(data.length === PAGE_SIZE);
      })
      .catch((e: Error) => setError(e.message))
      .finally(() => setLoading(false));
  }, [page]);

  return (
    <div>
      <h1 className="page-title" style={{ marginBottom: '1.5rem' }}>Users</h1>

      {loading && <p style={{ color: '#9ca3c8' }}>Loading users…</p>}
      {error && <p style={{ color: 'var(--danger)' }}>Error: {error}</p>}

      {!loading && !error && users.length === 0 && (
        <p style={{ color: '#5a5a80' }}>No users found.</p>
      )}

      {!loading && !error && users.length > 0 && (
        <div style={{ overflowX: 'auto', borderRadius: 8, border: '1px solid #1e1e42' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #1e1e42', background: '#0d0d2b', textAlign: 'left' }}>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Name</th>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Username</th>
              <th style={{ padding: '10px 8px', color: '#9ca3c8', fontWeight: 500, fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>Email</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u, idx) => (
              <tr key={u.id} style={{ borderBottom: '1px solid #1e1e42', background: idx % 2 === 0 ? 'transparent' : 'rgba(13,13,43,0.4)' }}>
                <td style={{ padding: '10px 8px', fontWeight: 500, color: '#e8e8ff' }}>{u.name}</td>
                <td style={{ padding: '10px 8px', color: '#9ca3c8' }}>@{u.userName}</td>
                <td style={{ padding: '10px 8px', color: '#9ca3c8' }}>{u.email}</td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      )}

      {!loading && !error && (
        <div style={{ display: 'flex', gap: 10, marginTop: '1.5rem', alignItems: 'center' }}>
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="btn-ghost"
            style={{ padding: '6px 14px', borderRadius: 4, opacity: page === 1 ? 0.4 : 1 }}
          >
            Prev
          </button>
          <span style={{ color: '#5a5a80' }}>Page {page}</span>
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={!hasMore}
            className="btn-ghost"
            style={{ padding: '6px 14px', borderRadius: 4, opacity: !hasMore ? 0.4 : 1 }}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
