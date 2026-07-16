import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import { useDevices, useRegisterDevice, type RegisterDeviceRequest, type RegisterDeviceResponse } from '../api';

function timeAgo(iso: string | null) {
  if (!iso) return 'never';
  const seconds = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
  if (seconds < 60) return `${seconds}s ago`;
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
  return `${Math.floor(seconds / 3600)}h ago`;
}

function RegisteredTokenBanner({ result, onDismiss }: { result: RegisterDeviceResponse; onDismiss: () => void }) {
  return (
    <div className="mb-6 rounded border border-amber-400 bg-amber-50 p-4 text-sm dark:border-amber-600 dark:bg-amber-950">
      <p className="font-medium text-amber-900 dark:text-amber-200">
        Device registered — copy this token now. It will not be shown again.
      </p>
      <p className="mt-2 text-amber-800 dark:text-amber-300">Device ID: <code>{result.deviceId}</code></p>
      <p className="mt-1 break-all font-mono text-amber-900 dark:text-amber-100">{result.authToken}</p>
      <button
        type="button"
        onClick={onDismiss}
        className="mt-3 rounded border border-amber-500 px-3 py-1 text-amber-900 dark:text-amber-200"
      >
        I've copied it
      </button>
    </div>
  );
}

export function DevicesPage() {
  const { data: devices, isLoading } = useDevices();
  const registerDevice = useRegisterDevice();
  const [registeredToken, setRegisteredToken] = useState<RegisterDeviceResponse | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RegisterDeviceRequest>();

  const onSubmit = handleSubmit((values) => {
    registerDevice.mutate(values, {
      onSuccess: (result) => {
        setRegisteredToken(result);
        reset();
      },
    });
  });

  return (
    <div className="min-h-screen bg-white p-8 dark:bg-slate-900">
      <div className="mx-auto max-w-3xl">
        <p className="mb-4">
          <Link className="text-[var(--color-primary)] underline" to="/dashboard">
            Back to dashboard
          </Link>
        </p>
        <h1 className="text-2xl font-semibold text-slate-900 dark:text-white">RFID Devices</h1>

        {registeredToken && (
          <RegisteredTokenBanner result={registeredToken} onDismiss={() => setRegisteredToken(null)} />
        )}

        <form onSubmit={onSubmit} className="mt-6 flex flex-wrap items-end gap-3 rounded border border-slate-200 p-4 dark:border-slate-700">
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Device name</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('deviceName', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Hardware id</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              placeholder="ESP32 MAC address"
              {...register('hardwareId', { required: true })}
            />
          </div>
          <div>
            <label className="block text-sm text-slate-600 dark:text-slate-300">Location</label>
            <input
              className="rounded border border-slate-300 px-2 py-1 dark:border-slate-600 dark:bg-slate-800 dark:text-white"
              {...register('location')}
            />
          </div>
          <button
            type="submit"
            disabled={registerDevice.isPending}
            className="rounded bg-[var(--color-primary)] px-4 py-2 text-white disabled:opacity-50"
          >
            Register device
          </button>
          {(errors.deviceName || errors.hardwareId) && (
            <p className="w-full text-sm text-red-600">Device name and hardware id are required.</p>
          )}
          {registerDevice.isError && (
            <p className="w-full text-sm text-red-600">Could not register — hardware id may already be in use.</p>
          )}
        </form>

        <div className="mt-6">
          {isLoading && <p className="text-slate-500">Loading devices…</p>}
          {devices?.length === 0 && <p className="text-slate-500">No devices registered yet.</p>}
          <ul className="space-y-2">
            {devices?.map((device) => (
              <li
                key={device.id}
                className="flex items-center justify-between rounded border border-slate-200 p-3 text-sm dark:border-slate-700"
              >
                <div>
                  <span
                    className={`mr-2 inline-block h-2.5 w-2.5 rounded-full ${device.isOnline ? 'bg-[#0ca30c]' : 'bg-slate-400'}`}
                    aria-hidden
                  />
                  <span className="font-medium text-slate-900 dark:text-white">{device.deviceName}</span>
                  <span className="ml-2 text-slate-500 dark:text-slate-400">
                    {device.location ?? 'No location'} · {device.hardwareId}
                  </span>
                </div>
                <div className="text-right text-slate-500 dark:text-slate-400">
                  <div>{device.isOnline ? 'Online' : 'Offline'}</div>
                  <div>Last heartbeat: {timeAgo(device.lastHeartbeatAtUtc)}</div>
                  {device.firmwareVersion && <div>Firmware v{device.firmwareVersion}</div>}
                </div>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
