import { createContext, useEffect, useState } from 'react';
import type {
  DownloadTracker,
  ServerEvent,
} from '../types';
import {
  createServerEventSource,
} from '../clients';

export const DownloadsContext = createContext<DownloadTracker[]>({} as DownloadTracker[]);

export function createDownloadsContext(): DownloadTracker[] {
  const [ downloads, setDownloads ] = useState<DownloadTracker[]>([]);

  useEffect(() => {
      const eventSource = createServerEventSource();
      eventSource.onmessage = event => {
        const data = JSON.parse(event.data) as ServerEvent;
        const { downloads } = data;
        setDownloads(downloads);
      };
  }, []);

  return downloads;
};
