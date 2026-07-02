import type { DownloadSearchResult, Title } from '../types';

export async function searchDownloads(searchTerm: string): Promise<DownloadSearchResult[]> {
  const query = searchTerm.trim();
  const response = await fetch(`/api/search?query=${query}`);
  if (!response.ok) {
    throw new Error(`Search request failed with status ${response.status}`);
  }
  const results = await response.json() as DownloadSearchResult[];
  return results;
};

export async function removeDownload(infoHash: string): Promise<void> {
  const response = await fetch(`/api/downloads/${infoHash}`, {
    method: 'DELETE',
  });
  if (!response.ok) {
    throw new Error(`Remove download request failed with status ${response.status}`);
  }
};

export async function startDownload(infoHash: string, title: Title): Promise<void> {
  const body = {
    infoHash,
    title,
  };
  const response = await fetch('/api/downloads', {
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
