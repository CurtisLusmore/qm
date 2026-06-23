import { useContext } from 'react';
import { ToastsContext } from '../contexts';

export default function useDispatchToast(): (message: string, type?: 'success' | 'error') => void {
  const dispatchToast = useContext(ToastsContext);
  return dispatchToast;
};
