import { ServerEventsContext } from '../contexts';
import type { ServerEventHandlerRegistration } from '../types';

export default function ServerEventsContextProvider({ children, value }: { children: React.ReactNode, value: ServerEventHandlerRegistration }): React.ReactElement {
  return (
    <ServerEventsContext.Provider value={value}>
      {children}
    </ServerEventsContext.Provider>
  );
};
