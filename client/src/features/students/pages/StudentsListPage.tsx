import { Link } from 'react-router-dom';
import { useStudents } from '../api';
import { useClasses } from '../../classes/api';

export function StudentsListPage() {
  const { data: students, isLoading } = useStudents();
  const { data: classes } = useClasses();

  const className = (classId: string) => classes?.find((c) => c.id === classId)?.name ?? classId;

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-3xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Students</h1>
          <Link
            to="/students/new"
            className="rounded bg-[var(--color-primary)] px-3 py-2 text-sm text-white"
          >
            Enroll student
          </Link>
        </div>

        <div className="mt-6">
          {isLoading && <p className="text-slate-500">Loading students…</p>}
          {students?.length === 0 && <p className="text-slate-500">No students yet.</p>}
          <ul className="space-y-2">
            {students?.map((student) => (
              <li key={student.id}>
                <Link
                  to={`/students/${student.id}`}
                  className="block rounded border border-slate-200 p-3 hover:bg-slate-50 dark:border-slate-700 dark:hover:bg-slate-800"
                >
                  <span className="font-medium text-slate-900 dark:text-white">
                    {student.firstName} {student.lastName}
                  </span>
                  <span className="ml-2 text-sm text-slate-500 dark:text-slate-400">
                    #{student.studentNumber} · {className(student.classId)}
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
