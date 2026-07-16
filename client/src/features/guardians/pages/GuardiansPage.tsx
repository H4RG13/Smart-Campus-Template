import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import {
  useGuardians,
  useCreateGuardian,
  useLinkGuardianToStudent,
  type CreateGuardianRequest,
  type LinkGuardianToStudentRequest,
} from '../api';
import { useStudents } from '../../students/api';

function LinkStudentForm({ guardianId }: { guardianId: string }) {
  const { data: students } = useStudents();
  const linkGuardian = useLinkGuardianToStudent(guardianId);
  const { register, handleSubmit, reset } = useForm<LinkGuardianToStudentRequest>();

  const onSubmit = handleSubmit((values) => {
    linkGuardian.mutate(values, { onSuccess: () => reset() });
  });

  return (
    <form onSubmit={onSubmit} className="mt-2 flex flex-wrap items-end gap-2 text-sm">
      <select
        className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
        {...register('studentId', { required: true })}
      >
        <option value="">Select a student…</option>
        {students?.map((s) => (
          <option key={s.id} value={s.id}>
            {s.firstName} {s.lastName}
          </option>
        ))}
      </select>
      <input
        type="text"
        placeholder="Relationship (e.g. Mother)"
        className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
        {...register('relationship', { required: true })}
      />
      <button type="submit" className="rounded bg-[var(--color-primary)] px-3 py-1.5 text-white">
        Link
      </button>
      {linkGuardian.isError && <p className="text-red-600">Could not link (already linked?).</p>}
    </form>
  );
}

export function GuardiansPage() {
  const { data: guardians, isLoading } = useGuardians();
  const { data: students } = useStudents();
  const createGuardian = useCreateGuardian();
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateGuardianRequest>();

  const onSubmit = handleSubmit((values) => {
    createGuardian.mutate(values, { onSuccess: () => reset() });
  });

  const studentName = (studentId: string) => {
    const s = students?.find((s) => s.id === studentId);
    return s ? `${s.firstName} ${s.lastName}` : studentId;
  };

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-3xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Guardians</h1>

        <form onSubmit={onSubmit} className="mt-6 flex flex-wrap items-end gap-3 rounded border border-slate-200 p-4 dark:border-slate-700">
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">First name</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('firstName', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Last name</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('lastName', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Phone</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('phone', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Email</label>
            <input
              type="email"
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('email', { required: true })}
            />
          </div>
          <button
            type="submit"
            disabled={createGuardian.isPending}
            className="rounded bg-[var(--color-primary)] px-4 py-2 text-white disabled:opacity-50"
          >
            Add guardian
          </button>
          {Object.keys(errors).length > 0 && (
            <p className="w-full text-sm text-red-600">All fields are required.</p>
          )}
        </form>

        <div className="mt-6">
          {isLoading && <p className="text-slate-500">Loading guardians…</p>}
          <ul className="space-y-3">
            {guardians?.map((guardian) => (
              <li key={guardian.id} className="rounded border border-slate-200 p-4 dark:border-slate-700">
                <button
                  type="button"
                  className="text-left font-medium text-slate-900 dark:text-white"
                  onClick={() => setExpandedId(expandedId === guardian.id ? null : guardian.id)}
                >
                  {guardian.firstName} {guardian.lastName} · {guardian.email}
                </button>
                <div className="mt-1 text-sm text-slate-500 dark:text-slate-400">
                  {guardian.studentLinks.length === 0
                    ? 'No students linked'
                    : guardian.studentLinks.map((l) => `${studentName(l.studentId)} (${l.relationship})`).join(', ')}
                </div>
                {expandedId === guardian.id && <LinkStudentForm guardianId={guardian.id} />}
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
