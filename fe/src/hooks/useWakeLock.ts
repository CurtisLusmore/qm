import { useState } from 'react';

type WakeLocker = {
  isLocked: boolean;
  requestWakeLock: () => Promise<void>;
  releaseWakeLock: () => Promise<void>;
}

export default function useWakeLock(): WakeLocker {
  const [ wakeLock, setWakeLock ] = useState<WakeLockSentinel | null>(null);

  async function requestWakeLock(): Promise<void> {
    if ('wakeLock' in navigator) {
      try {
        const sentinel = await (navigator as any).wakeLock.request('screen');
        setWakeLock(sentinel);
        sentinel?.addEventListener('release', () => {
          setWakeLock(null);
        });
      } catch (err) {
        console.error(`Failed to acquire wake lock: ${err}`);
      }
    } else {
      console.warn('Wake Lock API not supported in this browser.');
    }
  };

  async function releaseWakeLock(): Promise<void> {
    if (wakeLock) {
      try {
        await wakeLock.release();
        setWakeLock(null);
      } catch (err) {
        console.error(`Failed to release wake lock: ${err}`);
      }
    }
  };

  return {
    isLocked: wakeLock !== null,
    requestWakeLock,
    releaseWakeLock,
  };
};
