import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import { useClasses, useCreateClass, type CreateClassRequest } from '../api';
import { useTerms } from '../../academic-calendar/api';

export function ClassesPage() {
  const { data: classes, isLoading } = useClasses();
  const { data: terms } = useTerms();
  const createClass = useCreateClass();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateClassRequest>();

  const onSubmit = handleSubmit((values) => {
    createClass.mutate(values, { onSuccess: () => reset() });
  });

  const termName = (termId: string) => terms?.find((t) => t.id === termId)?.name ?? termId;

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-3xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Classes</h1>

        <form onSubmit={onSubmit} className="mt-6 flex flex-wrap items-end gap-3 rounded border border-slate-200 p-4 dark:border-slate-700">
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Class name</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('name', { required: 'Required' })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Term</label>
            <select
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('academicTermId', { required: 'Required' })}
            >
              <option value="">Select a term…</option>
              {terms?.map((term) => (
                <option key={term.id} value={term.id}>
                  {term.name}
                </option>
              ))}
            </select>
          </div>
          <button
            type="submit"
            disabled={createClass.isPending}
            className="rounded bg-[var(--color-primary)] px-4 py-2 text-white disabled:opacity-50"
          >
            Create class
          </button>
          {(errors.name || errors.academicTermId) && (
            <p className="w-full text-sm text-red-600">All fields are required.</p>
          )}
          {createClass.isError && <p className="w-full text-sm text-red-600">Could not create class.</p>}
        </form>

        <div className="mt-6">
          {isLoading && <p className="text-slate-500">Loading classes…</p>}
          {classes?.length === 0 && <p className="text-slate-500">No classes yet.</p>}
          <ul className="space-y-2">
            {classes?.map((c) => (
              <li key={c.id} className="rounded border border-slate-200 p-3 dark:border-slate-700">
                <span className="font-medium text-slate-900 dark:text-white">{c.name}</span>
                <span className="ml-2 text-sm text-slate-500 dark:text-slate-400">{termName(c.academicTermId)}</span>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
