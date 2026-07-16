import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useStudents } from '../../students/api';
import { useRecordAttendance, type RecordAttendanceRequest, type AttendanceStatus } from '../api';
import { ApiError } from '../../../shared/lib/apiClient';

interface FormValues {
  studentId: string;
  attendanceDate: string;
  entryMode: 'checkin' | 'absence';
  checkInTime: string;
  status: AttendanceStatus;
  notes: string;
}

const today = () => new Date().toISOString().slice(0, 10);

export function AttendanceEntryForm() {
  const { data: students } = useStudents();
  const recordAttendance = useRecordAttendance();
  const [entryMode, setEntryMode] = useState<'checkin' | 'absence'>('checkin');

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    defaultValues: { attendanceDate: today(), entryMode: 'checkin', status: 'Absent' },
  });

  const onSubmit = handleSubmit((values) => {
    const request: RecordAttendanceRequest = {
      studentId: values.studentId,
      attendanceDate: values.attendanceDate,
      ...(entryMode === 'checkin'
        ? { checkInTime: values.checkInTime }
        : { status: values.status, notes: values.notes || undefined }),
    };

    recordAttendance.mutate(request, { onSuccess: () => reset({ ...values, studentId: '' }) });
  });

  return (
    <form onSubmit={onSubmit} className="rounded border border-slate-200 p-4 dark:border-slate-700">
      <div className="flex flex-wrap items-end gap-3">
        <div>
          <label className="block text-sm text-slate-600 dark:text-slate-300">Student</label>
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
        </div>
        <div>
          <label className="block text-sm text-slate-600 dark:text-slate-300">Date</label>
          <input
            type="date"
            className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
            {...register('attendanceDate', { required: true })}
          />
        </div>
        <div>
          <label className="block text-sm text-slate-600 dark:text-slate-300">Entry type</label>
          <select
            className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
            value={entryMode}
            onChange={(e) => setEntryMode(e.target.value as 'checkin' | 'absence')}
          >
            <option value="checkin">Check-in (on time / late computed automatically)</option>
            <option value="absence">Absent / excused absence</option>
          </select>
        </div>

        {entryMode === 'checkin' ? (
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Check-in time</label>
            <input
              type="time"
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('checkInTime', { required: entryMode === 'checkin' })}
            />
          </div>
        ) : (
          <>
            <div>
              <label className="block text-sm text-slate-600 dark:text-slate-300">Status</label>
              <select
                className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
                {...register('status')}
              >
                <option value="Absent">Absent</option>
                <option value="ExcusedAbsence">Excused absence</option>
              </select>
            </div>
            <div>
              <label className="block text-sm text-slate-600 dark:text-slate-300">Notes</label>
              <input
                type="text"
                className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
                {...register('notes')}
              />
            </div>
          </>
        )}

        <button
          type="submit"
          disabled={recordAttendance.isPending}
          className="rounded bg-[var(--color-primary)] px-4 py-2 text-white disabled:opacity-50"
        >
          Record attendance
        </button>
      </div>

      {Object.keys(errors).length > 0 && (
        <p className="mt-2 text-sm text-red-600">Student, date, and check-in time are required.</p>
      )}
      {recordAttendance.isError && (
        <p className="mt-2 text-sm text-red-600">
          {recordAttendance.error instanceof ApiError
            ? 'Could not record attendance — a record may already exist for this student and date, or the date falls on a holiday.'
            : 'Something went wrong.'}
        </p>
      )}
    </form>
  );
}
