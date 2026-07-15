import { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import { useBranding, applyBrandingTheme } from '../config/useBranding';

export function RootLayout() {
  const { data: branding, isLoading, isError } = useBranding();

  useEffect(() => {
    if (branding) applyBrandingTheme(branding);
  }, [branding]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white dark:bg-slate-900">
        <p className="text-slate-500 dark:text-slate-400">Loading school configuration…</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-white dark:bg-slate-900">
        <p className="text-red-600">Could not reach the SmartCampus API.</p>
      </div>
    );
  }

  return <Outlet />;
}
