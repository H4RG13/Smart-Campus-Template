import { useState } from 'react';
import { Link } from 'react-router-dom';
import { apiClient } from '../../../shared/lib/apiClient';

function isoDaysAgo(days: number) {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toISOString().slice(0, 10);
}

export function ReportsPage() {
  const [fromDate, setFromDate] = useState(isoDaysAgo(30));
  const [toDate, setToDate] = useState(isoDaysAgo(0));
  const [isDownloading, setIsDownloading] = useState(false);
  const [error, setError] = useState(false);

  const downloadCsv = async () => {
    setIsDownloading(true);
    setError(false);
    try {
      const { blob, fileName } = await apiClient.download(
        `/reports/attendance?fromDate=${fromDate}&toDate=${toDate}&format=csv`,
      );
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(url);
    } catch {
      setError(true);
    } finally {
      setIsDownloading(false);
    }
  };

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-2xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Attendance Report</h1>

        <div className="mt-6 flex flex-wrap items-end gap-3 rounded border border-slate-200 p-4 dark:border-slate-700">
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">From</label>
            <input
              type="date"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">To</label>
            <input
              type="date"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
            />
          </div>
          <button
            type="button"
            onClick={downloadCsv}
            disabled={isDownloading}
            className="rounded bg-[var(--color-primary)] px-4 py-2 text-white disabled:opacity-50"
          >
            {isDownloading ? 'Downloading…' : 'Export CSV'}
          </button>
          {error && <p className="w-full text-sm text-red-600">Could not export the report.</p>}
        </div>
      </div>
    </div>
  );
}
