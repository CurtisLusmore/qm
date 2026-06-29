import { DownloadsContext } from '../contexts';
import type { DownloadTracker } from '../types';

export default function DownloadsContextProvider({ children, value }: { children: React.ReactNode, value: DownloadTracker[] }): React.ReactElement {
  return (
    <DownloadsContext.Provider value={value}>
      {children}
    </DownloadsContext.Provider>
  );
};
