import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { login, setToken, setUser } from '../api/auth';

interface FormState {
  email: string;
  password: string;
}

interface ValidationErrors {
  email?: string;
  password?: string;
}

function validate(form: FormState): ValidationErrors {
  const errors: ValidationErrors = {};
  if (!form.email.trim()) errors.email = 'Email is required.';
  if (!form.password) errors.password = 'Password is required.';
  return errors;
}

const inputStyle: React.CSSProperties = {
  width: '100%',
  padding: '8px 10px',
  borderRadius: 4,
  border: '1px solid #1e1e42',
  fontSize: '1rem',
  boxSizing: 'border-box',
  background: '#060612',
  color: '#e8e8ff',
};

function FormField({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
      <label style={{ fontWeight: 500, fontSize: '0.9rem', color: '#9ca3c8' }}>{label}</label>
      {children}
      {error && <span style={{ color: 'var(--danger)', fontSize: '0.82rem' }}>{error}</span>}
    </div>
  );
}

export function LoginPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<FormState>({ email: '', password: '' });
  const [errors, setErrors] = useState<ValidationErrors>({});
  const [apiError, setApiError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    setErrors((prev) => ({ ...prev, [name]: undefined }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const validationErrors = validate(form);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    setSubmitting(true);
    setApiError(null);
    try {
      const res = await login(form.email.trim(), form.password);
      setToken(res.token);
      setUser(res);
      navigate('/events');
    } catch (e) {
      setApiError((e as Error).message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div style={{ maxWidth: 440, margin: '0 auto' }}>
      <h1 style={{ marginBottom: '1.5rem' }}>Sign In</h1>

      {apiError && (
        <div style={{ background: 'var(--danger-bg)', color: 'var(--danger)', padding: '10px 14px', borderRadius: 6, marginBottom: '1rem', border: '1px solid rgba(255,56,96,0.3)' }}>
          {apiError}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <FormField label="Email *" error={errors.email}>
          <input type="email" name="email" value={form.email} onChange={handleChange} autoComplete="email" style={inputStyle} />
        </FormField>

        <FormField label="Password *" error={errors.password}>
          <input type="password" name="password" value={form.password} onChange={handleChange} autoComplete="current-password" style={inputStyle} />
        </FormField>

        <button
          type="submit"
          disabled={submitting}
          className="btn-primary"
          style={{ padding: '10px', borderRadius: 6, fontSize: '1rem' }}
        >
          {submitting ? 'Signing in…' : 'Sign In'}
        </button>
      </form>

      <p style={{ marginTop: '1rem', color: '#5a5a80' }}>
        No account yet? <Link to="/register">Register</Link>
      </p>
    </div>
  );
}