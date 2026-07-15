import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { apiClient } from '../../../shared/lib/apiClient';

export function AdminPage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['auth', 'admin-only'],
    queryFn: () => apiClient.get<{ message: string }>('/auth/admin-only'),
  });

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-2xl">
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Admin-only page</h1>
        {isLoading && <p className="mt-2 text-slate-500">Checking access…</p>}
        {isError && <p className="mt-2 text-red-600">Access denied.</p>}
        {data && <p className="mt-2 text-slate-600 dark:text-slate-300">{data.message}</p>}
        <p className="mt-6">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
      </div>
    </div>
  );
}
