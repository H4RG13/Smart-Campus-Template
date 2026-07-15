import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import { useStaffList, useCreateStaff, type CreateStaffRequest } from '../api';

export function StaffListPage() {
  const { data: staff, isLoading } = useStaffList();
  const createStaff = useCreateStaff();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateStaffRequest>();

  const onSubmit = handleSubmit((values) => {
    createStaff.mutate(values, { onSuccess: () => reset() });
  });

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-3xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">Staff</h1>

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
            <label className="block text-sm text-slate-600 dark:text-slate-300">Employee number</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('employeeNumber', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Position</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('position', { required: true })}
            />
          </div>
          <button
            type="submit"
            disabled={createStaff.isPending}
            className="rounded bg-[var(--color-primary)] px-4 py-2 text-white disabled:opacity-50"
          >
            Add staff
          </button>
          {Object.keys(errors).length > 0 && (
            <p className="w-full text-sm text-red-600">All fields are required.</p>
          )}
          {createStaff.isError && (
            <p className="w-full text-sm text-red-600">Could not add staff — check the employee number is unique.</p>
          )}
        </form>

        <div className="mt-6">
          {isLoading && <p className="text-slate-500">Loading staff…</p>}
          {staff?.length === 0 && <p className="text-slate-500">No staff yet.</p>}
          <ul className="space-y-2">
            {staff?.map((member) => (
              <li key={member.id} className="rounded border border-slate-200 p-3 dark:border-slate-700">
                <span className="font-medium text-slate-900 dark:text-white">
                  {member.firstName} {member.lastName}
                </span>
                <span className="ml-2 text-sm text-slate-500 dark:text-slate-400">
                  #{member.employeeNumber} · {member.position}
                </span>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
