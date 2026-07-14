import { useContext } from 'react';
import { ServerEventsContext } from '../contexts';
import type { ServerEventHandlerRegistration } from '../types';

export default function useServerEventHandlerRegistration(): ServerEventHandlerRegistration {
  return useContext(ServerEventsContext);
};
