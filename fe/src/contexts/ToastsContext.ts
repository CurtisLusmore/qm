import { createContext, useReducer } from 'react';

export type Severity = 'error' | 'info' | 'success' | 'warning';
export type ToastsManager = {
  toasts: Toast[];
  dispatchToast: (message: string, type?: Severity | null) => void;
  dismissToast: (id: string) => void;
}
export type DispatchToast = (message: string, type?: Severity | null) => void;
export const ToastsContext = createContext<DispatchToast>(() => {});

export type Toast = {
  id: string;
  message: string;
  type?: Severity | null;
};

type State = {
  toasts: Map<string, Toast>;
};

const initialState: State = {
  toasts: new Map(),
};

type AddToastAction = {
  type: 'add';
  message: string;
  toastType?: Severity | null;
};

type DismissToastAction = {
  type: 'dismiss';
  id: string;
};

type Action =
  | AddToastAction
  | DismissToastAction;

function reduce(state: State, action: Action): State {
  switch (action.type) {
    case 'add': {
      const id = crypto.randomUUID();
      const toast: Toast = {
        id,
        message: action.message,
        type: action.toastType,
      };
      const toasts = new Map(state.toasts);
      toasts.set(id, toast);
      return {
        ...state,
        toasts,
      };
    }

    case 'dismiss': {
      const toasts = new Map(state.toasts);
      toasts.delete(action.id);
      return {
        ...state,
        toasts,
      };
    }

    default:
      return state;
  }
}

export function createToastsContext(): ToastsManager {
  const [ state, dispatch ] = useReducer(reduce, initialState);

  function dispatchToast(message: string, type?: Severity | null): void {
    dispatch({ type: 'add', message, toastType: type });
  };

  function dismissToast(id: string): void {
    dispatch({ type: 'dismiss', id });
  };

  return {
    toasts: [ ...state.toasts.values() ],
    dispatchToast,
    dismissToast,
  };
};
