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

        <nav className="mt-6 flex flex-wrap gap-4 text-sm">
          <Link className="text-[var(--color-primary)] underline" to="/students">
            Students
          </Link>
          <Link className="text-[var(--color-primary)] underline" to="/attendance">
            Attendance
          </Link>
          {hasRole('Admin') && (
            <>
              <Link className="text-[var(--color-primary)] underline" to="/staff">
                Staff
              </Link>
              <Link className="text-[var(--color-primary)] underline" to="/classes">
                Classes
              </Link>
              <Link className="text-[var(--color-primary)] underline" to="/academic-calendar">
                Academic Calendar
              </Link>
              <Link className="text-[var(--color-primary)] underline" to="/admin">
                Admin-only page
              </Link>
            </>
          )}
        </nav>

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
