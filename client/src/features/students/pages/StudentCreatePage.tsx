import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { useCreateStudent, type CreateStudentRequest } from '../api';
import { useClasses } from '../../classes/api';

export function StudentCreatePage() {
  const navigate = useNavigate();
  const { data: classes } = useClasses();
  const createStudent = useCreateStudent();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateStudentRequest>();

  const onSubmit = handleSubmit((values) => {
    createStudent.mutate(values, {
      onSuccess: (student) => navigate(`/students/${student.id}`),
    });
  });

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-md">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/students">
            Back to students
          </Link>
        </p>
        <h1 className="mb-4 text-2xl font-semibold text-slate-900 dark:text-white">Enroll student</h1>

        <form onSubmit={onSubmit} className="space-y-3">
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">First name</label>
            <input
              className="w-full rounded border border-slate-300 px-2 py-1.5 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('firstName', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Last name</label>
            <input
              className="w-full rounded border border-slate-300 px-2 py-1.5 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('lastName', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Student number</label>
            <input
              className="w-full rounded border border-slate-300 px-2 py-1.5 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('studentNumber', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Date of birth</label>
            <input
              type="date"
              className="w-full rounded border border-slate-300 px-2 py-1.5 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('dateOfBirth', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Class</label>
            <select
              className="w-full rounded border border-slate-300 px-2 py-1.5 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('classId', { required: true })}
            >
              <option value="">Select a class…</option>
              {classes?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          {Object.keys(errors).length > 0 && (
            <p className="text-sm text-red-600">All fields are required.</p>
          )}
          {createStudent.isError && (
            <p className="text-sm text-red-600">Could not enroll student — check the student number is unique.</p>
          )}

          <button
            type="submit"
            disabled={createStudent.isPending}
            className="w-full rounded bg-[var(--color-primary)] px-3 py-2 font-medium text-white disabled:opacity-50"
          >
            {createStudent.isPending ? 'Enrolling…' : 'Enroll student'}
          </button>
        </form>
      </div>
    </div>
  );
}
