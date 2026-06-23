import { useEffect, useState } from 'react';
import type { DownloadSearchResult, DownloadTracker, Title } from '../types';

export async function searchDownloads(title: Title): Promise<DownloadSearchResult[]> {
  const query = `${title.name} ${title.year ?? ''}`.trim();
  const response = await fetch(`http://localhost:5138/search?query=${query}`);
  if (!response.ok) {
    throw new Error(`Search request failed with status ${response.status}`);
  }
  const results = await response.json() as DownloadSearchResult[];
  return results;
};

export async function startDownload(infoHash: string, title: Title): Promise<void> {
  const body = {
    infoHash,
    title,
  };
  const response = await fetch('http://localhost:5138/downloads', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  });
  if (!response.ok) {
    throw new Error(`Download request failed with status ${response.status}`);
  }
};

export function useDownloadTrackers(): DownloadTracker[] {
  const [ trackers, setTrackers ] = useState<DownloadTracker[]>([]);

  useEffect(() => {
    const eventSource = new EventSource('http://localhost:5138/subscribe');
    
    eventSource.onmessage = (event) => {
      const trackers = JSON.parse(event.data) as { downloads: DownloadTracker[] };
      setTrackers(trackers.downloads);
    };
  }, []);

  return trackers;
}