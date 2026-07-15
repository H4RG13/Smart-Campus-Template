import { useEffect } from 'react';
import { useBranding, applyBrandingTheme } from './config/useBranding';

function App() {
  const { data: branding, isLoading, isError } = useBranding();

  useEffect(() => {
    if (branding) applyBrandingTheme(branding);
  }, [branding]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-white dark:bg-slate-900">
      <div className="text-center">
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">
          {isLoading && 'Loading school configuration…'}
          {isError && 'Could not reach the SmartCampus API.'}
          {branding && branding.schoolName}
        </h1>
        {branding && (
          <p className="mt-2 text-slate-500 dark:text-slate-400">
            SmartCampus Template — scaffold running.
          </p>
        )}
      </div>
    </div>
  );
}

export default App;
