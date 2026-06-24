import { useCallback } from 'react';

export default function useAutofocus() {
  return useCallback((node: HTMLInputElement) => {
    if (node !== null) {
      const inputmode = node.getAttribute('inputmode') || 'text';
      if (inputmode === 'none') return;
      node.setAttribute('inputmode', 'none');
      setTimeout(() => {
        node.focus();
        setTimeout(() => node.setAttribute('inputmode', inputmode), 50);
      }, 50);
    }
  }, []);
};
