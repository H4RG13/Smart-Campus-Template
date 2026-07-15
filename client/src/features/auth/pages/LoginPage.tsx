import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { useLogin } from '../api';
import { ApiError } from '../../../shared/lib/apiClient';

interface LoginFormValues {
  email: string;
  password: string;
}

export function LoginPage() {
  const navigate = useNavigate();
  const login = useLogin();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>();

  const onSubmit = handleSubmit((values) => {
    login.mutate(values, {
      onSuccess: () => navigate('/dashboard'),
    });
  });

  return (
    <div className="flex min-h-screen items-center justify-center bg-white dark:bg-slate-900">
      <form
        onSubmit={onSubmit}
        className="w-full max-w-sm rounded-lg border border-slate-200 p-6 dark:border-slate-700"
      >
        <h1 className="mb-4 text-xl font-semibold text-slate-900 dark:text-white">Sign in</h1>

        <label className="mb-1 block text-sm text-slate-600 dark:text-slate-300" htmlFor="email">
          Email
        </label>
        <input
          id="email"
          type="email"
          className="mb-3 w-full rounded border border-slate-300 px-3 py-2 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
          {...register('email', { required: 'Email is required' })}
        />
        {errors.email && <p className="mb-2 text-sm text-red-600">{errors.email.message}</p>}

        <label className="mb-1 block text-sm text-slate-600 dark:text-slate-300" htmlFor="password">
          Password
        </label>
        <input
          id="password"
          type="password"
          className="mb-3 w-full rounded border border-slate-300 px-3 py-2 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
          {...register('password', { required: 'Password is required' })}
        />
        {errors.password && <p className="mb-2 text-sm text-red-600">{errors.password.message}</p>}

        {login.isError && (
          <p className="mb-3 text-sm text-red-600">
            {login.error instanceof ApiError ? 'Invalid email or password.' : 'Something went wrong.'}
          </p>
        )}

        <button
          type="submit"
          disabled={login.isPending}
          className="w-full rounded bg-[var(--color-primary)] px-3 py-2 font-medium text-white disabled:opacity-50"
        >
          {login.isPending ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}
