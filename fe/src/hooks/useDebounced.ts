import { useRef } from 'react';

export default function useDebounced<T extends (...params: any[]) => Promise<any>>(
  fn: T,
  delay: number
): (...params: Parameters<T>) => Promise<Awaited<ReturnType<T>>> {
  const timeoutRef = useRef<number | null>(null);
  const prevRejectRef = useRef<((value: Awaited<ReturnType<T>> | undefined) => void) | null>(null);

  return (...params: Parameters<T>) => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current);
    if (prevRejectRef.current) prevRejectRef.current(undefined as any); 

    return new Promise<Awaited<ReturnType<T>>>((resolve, reject) => {
      prevRejectRef.current = reject as (value: Awaited<ReturnType<T>> | undefined) => void;
      timeoutRef.current = window.setTimeout(() => {
        fn(...params).then(resolve);
      }, delay);
    });
  };
};
