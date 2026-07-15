import { Link, useParams } from 'react-router-dom';
import { useStudent } from '../api';
import { useClasses } from '../../classes/api';

export function StudentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: student, isLoading, isError } = useStudent(id);
  const { data: classes } = useClasses();

  const className = classes?.find((c) => c.id === student?.classId)?.name;

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-md">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/students">
            Back to students
          </Link>
        </p>

        {isLoading && <p className="text-slate-500">Loading…</p>}
        {isError && <p className="text-red-600">Student not found.</p>}

        {student && (
          <>
            <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">
              {student.firstName} {student.lastName}
            </h1>
            <dl className="mt-4 space-y-2 text-sm">
              <div>
                <dt className="text-slate-500 dark:text-slate-400">Student number</dt>
                <dd className="text-slate-900 dark:text-white">{student.studentNumber}</dd>
              </div>
              <div>
                <dt className="text-slate-500 dark:text-slate-400">Class</dt>
                <dd className="text-slate-900 dark:text-white">{className ?? student.classId}</dd>
              </div>
              <div>
                <dt className="text-slate-500 dark:text-slate-400">Date of birth</dt>
                <dd className="text-slate-900 dark:text-white">{student.dateOfBirth}</dd>
              </div>
              <div>
                <dt className="text-slate-500 dark:text-slate-400">RFID tag</dt>
                <dd className="text-slate-900 dark:text-white">{student.rfidTagId ?? 'Not assigned'}</dd>
              </div>
              <div>
                <dt className="text-slate-500 dark:text-slate-400">Status</dt>
                <dd className="text-slate-900 dark:text-white">{student.isActive ? 'Active' : 'Inactive'}</dd>
              </div>
            </dl>
          </>
        )}
      </div>
    </div>
  );
}
