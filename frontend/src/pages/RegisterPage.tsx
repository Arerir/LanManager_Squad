import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { registerUser } from '../api/users';
import type { RegisterRequest } from '../api/users';

interface FormState {
  email: string;
  password: string;
  userName: string;
  name: string;
}

interface ValidationErrors {
  email?: string;
  password?: string;
  userName?: string;
  name?: string;
}

const EMPTY: FormState = { email: '', password: '', userName: '', name: '' };

function validate(form: FormState): ValidationErrors {
  const errors: ValidationErrors = {};
  if (!form.email.trim()) {
    errors.email = 'Email is required.';
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'Enter a valid email address.';
  }
  if (!form.password) {
    errors.password = 'Password is required.';
  } else if (form.password.length < 8) {
    errors.password = 'Password must be at least 8 characters.';
  }
  if (!form.userName.trim()) errors.userName = 'Username is required.';
  if (!form.name.trim()) errors.name = 'Name is required.';
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

export function RegisterPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<FormState>(EMPTY);
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
    const payload: RegisterRequest = {
      email: form.email.trim(),
      password: form.password,
      userName: form.userName.trim(),
      name: form.name.trim(),
    };
    setSubmitting(true);
    setApiError(null);
    try {
      const user = await registerUser(payload);
      localStorage.setItem('currentUser', JSON.stringify(user));
      navigate('/profile');
    } catch (e) {
      setApiError((e as Error).message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div style={{ maxWidth: 440, margin: '0 auto' }}>
      <h1 style={{ marginBottom: '1.5rem' }}>Create Account</h1>

      {apiError && (
        <div style={{ background: 'var(--danger-bg)', color: 'var(--danger)', padding: '10px 14px', borderRadius: 6, marginBottom: '1rem', border: '1px solid rgba(255,56,96,0.3)' }}>
          {apiError}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <FormField label="Name *" error={errors.name}>
          <input name="name" value={form.name} onChange={handleChange} autoComplete="name" style={inputStyle} />
        </FormField>

        <FormField label="Username *" error={errors.userName}>
          <input name="userName" value={form.userName} onChange={handleChange} autoComplete="username" style={inputStyle} />
        </FormField>

        <FormField label="Email *" error={errors.email}>
          <input type="email" name="email" value={form.email} onChange={handleChange} autoComplete="email" style={inputStyle} />
        </FormField>

        <FormField label="Password *" error={errors.password}>
          <input type="password" name="password" value={form.password} onChange={handleChange} autoComplete="new-password" style={inputStyle} />
        </FormField>

        <button
          type="submit"
          disabled={submitting}
          className="btn-primary"
          style={{ padding: '10px', borderRadius: 6, fontSize: '1rem' }}
        >
          {submitting ? 'Creating account…' : 'Register'}
        </button>
      </form>

      <p style={{ marginTop: '1rem', color: '#5a5a80' }}>
        Already have an account? <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
}
