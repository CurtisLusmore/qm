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
