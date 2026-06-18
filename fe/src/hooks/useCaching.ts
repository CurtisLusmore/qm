import { useRef } from 'react';

export default function useCaching<T extends (...params: any[]) => Promise<any>>(
  fn: T
): (...params: Parameters<T>) => Promise<Awaited<ReturnType<T>>> {
  const paramsRef = useRef<Parameters<T> | null>(null);
  const resultRef = useRef<Awaited<ReturnType<T>> | null>(null);

  return async (...params: Parameters<T>) => {
    if (paramsRef.current && resultRef.current && JSON.stringify(params) === JSON.stringify(paramsRef.current)) {
      return Promise.resolve(resultRef.current as Awaited<ReturnType<T>>);
    } else {
      paramsRef.current = params;
      const result = resultRef.current = await fn(...params);
      return result;
    }
  };
};
