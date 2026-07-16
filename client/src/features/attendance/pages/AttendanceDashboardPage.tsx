import { Link } from 'react-router-dom';
import { useAttendanceRecords, useAttendanceSummary, type AttendanceStatus } from '../api';
import { useStudents } from '../../students/api';
import { AttendanceSummaryChart } from '../components/AttendanceSummaryChart';
import { AttendanceEntryForm } from './AttendanceEntryForm';

const STATUS_STYLES: Record<AttendanceStatus, string> = {
  OnTime: 'text-[#0ca30c]',
  Late: 'text-[#c98500]',
  ExcusedAbsence: 'text-[#ec835a]',
  Absent: 'text-[#d03b3b]',
};

const STATUS_LABELS: Record<AttendanceStatus, string> = {
  OnTime: 'On time',
  Late: 'Late',
  ExcusedAbsence: 'Excused absence',
  Absent: 'Absent',
};

function isoDaysAgo(days: number) {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toISOString().slice(0, 10);
}

export function AttendanceDashboardPage() {
  const fromDate = isoDaysAgo(13);
  const toDate = isoDaysAgo(0);

  const { data: summary, isLoading: summaryLoading } = useAttendanceSummary(fromDate, toDate);
  const { data: records, isLoading: recordsLoading } = useAttendanceRecords({ fromDate, toDate });
  const { data: students } = useStudents();

  const studentName = (studentId: string) => {
    const student = students?.find((s) => s.id === studentId);
    return student ? `${student.firstName} ${student.lastName}` : studentId;
  };

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-4xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Attendance</h1>

        <div className="mt-6">
          <AttendanceEntryForm />
        </div>

        <div className="mt-8">
          <h2 className="mb-2 text-lg font-medium text-slate-900 dark:text-white">Last 14 days</h2>
          {summaryLoading && <p className="text-slate-500">Loading summary…</p>}
          {summary && <AttendanceSummaryChart data={summary} />}
        </div>

        <div className="mt-8">
          <h2 className="mb-2 text-lg font-medium text-slate-900 dark:text-white">Recent records</h2>
          {recordsLoading && <p className="text-slate-500">Loading records…</p>}
          {records?.length === 0 && <p className="text-slate-500">No records in this range.</p>}
          <ul className="space-y-2">
            {records?.map((record) => (
              <li
                key={record.id}
                className="flex items-center justify-between rounded border border-slate-200 p-3 text-sm dark:border-slate-700"
              >
                <span className="text-slate-900 dark:text-white">
                  {studentName(record.studentId)} — {record.attendanceDate}
                </span>
                <span className={`font-medium ${STATUS_STYLES[record.status]}`}>
                  {STATUS_LABELS[record.status]}
                </span>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
