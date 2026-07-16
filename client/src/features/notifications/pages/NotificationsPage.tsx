import { Link } from 'react-router-dom';
import { useNotificationLogs } from '../api';
import { useStudents } from '../../students/api';
import { useGuardians } from '../../guardians/api';

export function NotificationsPage() {
  const { data: logs, isLoading } = useNotificationLogs();
  const { data: students } = useStudents();
  const { data: guardians } = useGuardians();

  const studentName = (id: string) => {
    const s = students?.find((s) => s.id === id);
    return s ? `${s.firstName} ${s.lastName}` : id;
  };
  const guardianName = (id: string) => {
    const g = guardians?.find((g) => g.id === id);
    return g ? `${g.firstName} ${g.lastName}` : id;
  };

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-3xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Notification Logs</h1>

        <div className="mt-6">
          {isLoading && <p className="text-slate-500">Loading logs…</p>}
          {logs?.length === 0 && <p className="text-slate-500">No notifications sent yet.</p>}
          <ul className="space-y-2">
            {logs?.map((log) => (
              <li key={log.id} className="flex items-center justify-between rounded border border-slate-200 p-3 text-sm dark:border-slate-700">
                <span className="text-slate-900 dark:text-white">
                  {studentName(log.studentId)} → {guardianName(log.guardianId)} · {log.triggerReason}
                </span>
                <span className={log.status === 'Sent' ? 'text-[#0ca30c]' : 'text-[#d03b3b]'}>
                  {log.status}
                </span>
                <span className="text-slate-500 dark:text-slate-400">
                  {new Date(log.sentAtUtc).toLocaleString()}
                </span>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
