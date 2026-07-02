export interface Collection {
  loaded: boolean;
  movies: Title[];
  series: Title[];
  recentlyAdded: Title[];
  get(titleId: string): Title | undefined;
  add(title: Title): Promise<void>;
  remove(titleId: string): Promise<void>;
  markWatched(titleId: string): Promise<void>;
  check<T extends TitleSummary>(searchResults: T[]): (T & CollectionStatus)[];
  check<T extends TitleSummary>(searchResult: T): T & CollectionStatus;
}

export type CollectionStatus = {
  inCollection?: boolean;
  addedOn?: Date;
  watched?: boolean;
  lastWatched?: Date;
  downloaded?: boolean;
}

export type DownloadSearchResult = {
  infoHash: string;
  name: string;
  sizeBytes: number;
  seeders: number;
}

export type DownloadTracker = {
  infoHash: string;
  title: Title;
  status: DownloadTrackerStatus;
  error?: string;
  files?: FileTracker[];
  downloadedBytes: number;
  targetBytes: number;
  totalBytes: number;
  partialProgressPercent: number;
  targetProgressPercent: number;
  totalProgressPercent: number;
  bytesPerSecond: number;
  seeds: number;
}

export type DownloadTrackerStatus =
  | 'Received'
  | 'DownloadingTorrentFile'
  | 'DownloadTorrentFileFailed'
  | 'DownloadedTorrentFile'
  | 'AddedTorrent'
  | 'StartedTorrent'
  | 'InitializingTorrent'
  | 'DownloadingTorrent'
  | 'PausedTorrent'
  | 'DownloadTorrentFailed'
  | 'StoppingTorrent'
  | 'DownloadedTorrent'
  | 'SortingFiles'
  | 'ManualSortingRequired'
  | 'Completed'
  | 'Removing';

export type Episode = Title & {
  seasonNumber: number;
  episodeNumber: number;
}

export type FilePriority =
  | 'Skip'
  | 'Low'
  | 'Normal'
  | 'High';

export type FileTracker = {
  path: string;
  priority: FilePriority;
  downloadedBytes: number;
  totalBytes: number;
  progressPercent: number;
}

export type Movie = Title;

export type Series = Title & {
  episodes: Episode[];
}

export interface ServerCollection {
  movies: Movie[];
  series: Series[];
}

export type TitleType =
  | 'movie'
  | 'series'
  | 'episode';

export type TitleSummary = {
  id: string;
  type: TitleType;
  name: string;
  year?: number;
  imageUrl: string;
}

export type PersonSummary = {
  id: string;
  name: string;
}

export type ServerEvent = {
  downloads: DownloadTracker[];
}

export type Title = TitleSummary & CollectionStatus & {
  endYear?: number;
  releaseDate?: Date;
  plot: string;
  genres: string[];
  trailerUrl?: string;
  runtimeSeconds?: number;
  classification: string;
  ratings: {
    rating: number;
    count: number;
  };
  cast: PersonSummary[];
  directors: PersonSummary[];
  writers: PersonSummary[];
}
