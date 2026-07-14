import type { DownloadPatch, DownloadSearchResult, Title } from '../types';

export async function searchDownloads(searchTerm: string): Promise<DownloadSearchResult[]> {
  const query = searchTerm.trim();
  const response = await fetch(`/api/search?query=${query}`);
  if (!response.ok) {
    throw new Error(`Search request failed with status ${response.status}`);
  }
  const results = await response.json() as DownloadSearchResult[];
  return results;
};

export async function pauseDownload(infoHash: string): Promise<void> {
  const response = await fetch(`/api/downloads/${infoHash}`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ status: 'DownloadPaused' } as DownloadPatch),
  });
  if (!response.ok) {
    throw new Error(`Pause download request failed with status ${response.status}`);
  }
};

export async function resumeDownload(infoHash: string): Promise<void> {
  const response = await fetch(`/api/downloads/${infoHash}`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ status: 'DownloadingFiles' } as DownloadPatch),
  });
  if (!response.ok) {
    throw new Error(`Resume download request failed with status ${response.status}`);
  }
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
