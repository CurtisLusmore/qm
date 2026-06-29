import { useContext } from 'react';
import { DownloadsContext } from '../contexts';
import type { DownloadTracker } from '../types';

export default function useDownloads(): DownloadTracker[] {
  return useContext(DownloadsContext);
};
