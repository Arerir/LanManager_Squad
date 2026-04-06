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

      {loading && <p>Loading users…</p>}
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}

      {!loading && !error && users.length === 0 && (
        <p style={{ color: '#888' }}>No users found.</p>
      )}

      {!loading && !error && users.length > 0 && (
        <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #dee2e6', textAlign: 'left' }}>
              <th style={{ padding: '10px 8px' }}>Name</th>
              <th style={{ padding: '10px 8px' }}>Username</th>
              <th style={{ padding: '10px 8px' }}>Email</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} style={{ borderBottom: '1px solid #dee2e6' }}>
                <td style={{ padding: '10px 8px', fontWeight: 500 }}>{u.name}</td>
                <td style={{ padding: '10px 8px', color: '#555' }}>@{u.userName}</td>
                <td style={{ padding: '10px 8px', color: '#555' }}>{u.email}</td>
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
            style={{ padding: '6px 14px', borderRadius: 4, cursor: page === 1 ? 'default' : 'pointer' }}
          >
            Prev
          </button>
          <span style={{ color: '#555' }}>Page {page}</span>
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={!hasMore}
            style={{ padding: '6px 14px', borderRadius: 4, cursor: !hasMore ? 'default' : 'pointer' }}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
