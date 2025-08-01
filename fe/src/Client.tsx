const baseUrl = '';

export interface Torrent {
  infoHash: string;
  name: string;
  state: State;
  seeders: number;
  downloadedBytes: number;
  targetBytes: number;
  sizeBytes: number;
  partialProgressPercent: number;
  targetPercent: number;
  progressPercent: number;
  bytesPerSecond: number;
  files: TorrentFile[];
};

export interface TorrentFile {
  path: string;
  downloadedBytes: number;
  sizeBytes: number;
  progressPercent: number;
  priority: Priority;
};

export interface TorrentSearchResult {
  infoHash: string;
  name: string;
  seeders: number;
  sizeBytes: number;
  numFiles: number;
};

export interface TorrentPatch {
  state?: State;
  files?: TorrentFilePatch[];
};

export interface TorrentFilePatch {
  path: string;
  priority: Priority;
};

export type State = 'Initializing'
  | 'Downloading'
  | 'Paused'
  | 'Completing'
  | 'Complete'
  | 'Removing'
  | 'Removed'
  | 'Error';
export type Priority = 'Skip' | 'Normal' | 'High';

const api = {
  request(path: string, method: string, payload: any | null): Promise<any> {
    const headers = payload !== null ? { 'Content-Type': 'application/json' } : undefined;
    const body = payload !== null ? JSON.stringify(payload) : undefined;
    return fetch(`${baseUrl}${path}`, { method, headers, body });
  },
  get(path: string): Promise<any> {
    return this.request(path, 'GET', null);
  },
  delete(path: string): Promise<any> {
    return this.request(path, 'DELETE', null);
  },
  patch(path: string, payload: any | null): Promise<any> {
    return this.request(path, 'PATCH', payload);
  },
  post(path: string, payload: any | null = null): Promise<any> {
    return this.request(path, 'POST', payload);
  },
};

export async function searchTorrents(search: string): Promise<TorrentSearchResult[]> {
  const resp = await api.get(`/api/search?terms=${encodeURI(search)}`);
  const results = await resp.json() as TorrentSearchResult[];
  return results;
};

export async function getTorrents(): Promise<Torrent[]> {
  const resp = await api.get('/api/torrents');
  const torrents = await resp.json() as Torrent[];
  return torrents;
};

export async function getTorrent(infoHash: string): Promise<Torrent> {
  const resp = await api.get(`/api/torrents/${infoHash}`);
  const torrent = await resp.json() as Torrent;
  return torrent;
};

export async function saveTorrent(infoHash: string, name: string): Promise<void> {
  await api.post('/api/torrents', { infoHash, name });
};

export async function removeTorrent(infoHash: string): Promise<void> {
  await api.delete(`/api/torrents/${infoHash}`);
};

export async function updateTorrent(infoHash: string, patch: TorrentPatch): Promise<void> {
  await api.patch(`/api/torrents/${infoHash}`, patch);
};
