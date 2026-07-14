import { createContext, useCallback, useEffect, useState } from 'react';
import type {
  ServerEvent,
  ServerEventHandler,
  ServerEventHandlerRegistration,
} from '../types';
import {
  createServerEventSource,
} from '../clients';

export const ServerEventsContext = createContext<ServerEventHandlerRegistration>(() => (() => {}));

export function createServerEventsContext(): ServerEventHandlerRegistration {
  const [ eventSource, setEventSource ] = useState<EventSource | null>(null);

  useEffect(() => {
      const eventSource = createServerEventSource();
      setEventSource(eventSource);
  }, []);

  const addEventListener = useCallback((listener: ServerEventHandler): (() => void) => {
    if (!eventSource) return () => {};
    function handleMessage(event: MessageEvent) {
      const data = JSON.parse(event.data) as ServerEvent;
      listener(data);
    };
    eventSource.addEventListener('message', handleMessage);
    return () => eventSource.removeEventListener('message', handleMessage);
  }, [ eventSource ]);

  return addEventListener;
};
