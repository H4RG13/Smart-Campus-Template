import { Link } from 'react-router-dom';
import { useAuthStore } from '../store';
import { useBranding } from '../../../config/useBranding';

export function DashboardPage() {
  const { email, roles, logout, hasRole } = useAuthStore();
  const { data: branding } = useBranding();

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-2xl">
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">
          {branding?.schoolName ?? 'SmartCampus'}
        </h1>
        <p className="mt-2 text-slate-600 dark:text-slate-300">
          Signed in as <strong>{email}</strong> ({roles.join(', ')})
        </p>

        {hasRole('Admin') && (
          <p className="mt-4">
            <Link className="text-[var(--color-primary)] underline" to="/admin">
              Go to Admin-only page
            </Link>
          </p>
        )}

        <button
          type="button"
          onClick={logout}
          className="mt-6 rounded border border-slate-300 px-3 py-2 text-sm dark:border-slate-600 dark:text-white"
        >
          Log out
        </button>
      </div>
    </div>
  );
}
