import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import {
  useTerms,
  useCreateTerm,
  useAddException,
  type CreateTermRequest,
  type CalendarExceptionType,
} from '../api';

interface ExceptionFormValues {
  date: string;
  type: CalendarExceptionType;
  description?: string;
}

function AddExceptionForm({ termId }: { termId: string }) {
  const addException = useAddException(termId);
  const { register, handleSubmit, reset } = useForm<ExceptionFormValues>();

  const onSubmit = handleSubmit((values) => {
    addException.mutate(values, { onSuccess: () => reset() });
  });

  return (
    <form onSubmit={onSubmit} className="mt-2 flex flex-wrap items-end gap-2 text-sm">
      <div>
        <label className="block text-slate-500 dark:text-slate-400">Date</label>
        <input
          type="date"
          required
          className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
          {...register('date', { required: true })}
        />
      </div>
      <div>
        <label className="block text-slate-500 dark:text-slate-400">Type</label>
        <select
          className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
          {...register('type', { required: true })}
        >
          <option value="Holiday">Holiday</option>
          <option value="HalfDay">Half Day</option>
          <option value="SpecialSchedule">Special Schedule</option>
        </select>
      </div>
      <div>
        <label className="block text-slate-500 dark:text-slate-400">Description</label>
        <input
          type="text"
          className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
          {...register('description')}
        />
      </div>
      <button
        type="submit"
        disabled={addException.isPending}
        className="rounded bg-[var(--color-primary)] px-3 py-1.5 text-white disabled:opacity-50"
      >
        Add exception
      </button>
      {addException.isError && <p className="text-red-600">Could not add exception.</p>}
    </form>
  );
}

export function AcademicCalendarPage() {
  const { data: terms, isLoading } = useTerms();
  const createTerm = useCreateTerm();
  const [expandedTermId, setExpandedTermId] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateTermRequest>();

  const onSubmit = handleSubmit((values) => {
    createTerm.mutate(values, { onSuccess: () => reset() });
  });

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-3xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Academic Calendar</h1>

        <form onSubmit={onSubmit} className="mt-6 flex flex-wrap items-end gap-3 rounded border border-slate-200 p-4 dark:border-slate-700">
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Term name</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('name', { required: 'Required' })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Start date</label>
            <input
              type="date"
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('startDate', { required: 'Required' })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">End date</label>
            <input
              type="date"
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('endDate', { required: 'Required' })}
            />
          </div>
          <button
            type="submit"
            disabled={createTerm.isPending}
            className="rounded bg-[var(--color-primary)] px-4 py-2 text-white disabled:opacity-50"
          >
            Create term
          </button>
          {(errors.name || errors.startDate || errors.endDate) && (
            <p className="w-full text-sm text-red-600">All fields are required.</p>
          )}
          {createTerm.isError && <p className="w-full text-sm text-red-600">Could not create term.</p>}
        </form>

        <div className="mt-6">
          {isLoading && <p className="text-slate-500">Loading terms…</p>}
          {terms?.length === 0 && <p className="text-slate-500">No terms yet.</p>}
          <ul className="space-y-3">
            {terms?.map((term) => (
              <li key={term.id} className="rounded border border-slate-200 p-4 dark:border-slate-700">
                <button
                  type="button"
                  className="text-left font-medium text-slate-900 dark:text-white"
                  onClick={() => setExpandedTermId(expandedTermId === term.id ? null : term.id)}
                >
                  {term.name} ({term.startDate} → {term.endDate})
                </button>
                {expandedTermId === term.id && <AddExceptionForm termId={term.id} />}
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
