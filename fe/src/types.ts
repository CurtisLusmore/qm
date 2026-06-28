export interface Collection {
  loaded: boolean;
  movies: Title[];
  series: Title[];
  recentlyAdded: Title[];
  downloads: DownloadTracker[];
  get(titleId: string): CollectionStatus<Title> | undefined;
  add(title: Title): Promise<void>;
  remove(titleId: string): Promise<void>;
  markWatched(titleId: string): Promise<void>;
  check<T extends TitleSummary>(searchResults: T[]): CollectionStatus<T>[];
  check<T extends TitleSummary>(searchResult: T): CollectionStatus<T>;
}

export type CollectionStatus<T> = T & {
  inCollection: boolean;
  addedOn?: Date;
  watched: boolean;
  lastWatched?: Date;
  downloadStatus: DownloadStatus;
}

export type DownloadStatus = 'not_downloaded' | 'downloading' | 'downloaded';

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
  | 'Completed';

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
  movies: CollectionStatus<Movie>[];
  series: CollectionStatus<Series>[];
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

export type Title = TitleSummary & {
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
