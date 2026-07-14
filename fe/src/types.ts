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
  addedOn?: string;
  watched?: boolean;
  lastWatched?: string;
  downloaded?: boolean;
}

export type DownloadSearchResult = {
  infoHash: string;
  name: string;
  sizeBytes: number;
  seeders: number;
}

export type DownloadTracker = {
  name: string;
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

export type DownloadPatch = {
  status: DownloadTrackerStatus;
}

export type DownloadTrackerStatus =
  | 'Received'
  | 'DownloadingTorrentFile'
  | 'AddingTorrent'
  | 'MappingFiles'
  | 'LoadingFastResume'
  | 'StartingTorrent'
  | 'LoadingMetadata'
  | 'DownloadingFiles'
  | 'DownloadPaused'
  | 'StoppingTorrent'
  | 'Completed'
  | 'Deleting'
  | 'Failed';

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

export type TitleType =
  | 'Movie'
  | 'Series'
  | 'Episode';

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

export type DownloadAddedEvent = {
  type: 'DownloadAdded';
  download: DownloadTracker;
}

export type DownloadProgressEvent = {
  type: 'DownloadProgress';
  download: DownloadTracker;
}

export type DownloadCompletedEvent = {
  type: 'DownloadCompleted';
  download: DownloadTracker;
}

export type DownloadRemoved = {
  type: 'DownloadRemoved';
  infoHash: string;
}

export type DownloadFailedEvent = {
  type: 'DownloadFailed';
  download: DownloadTracker;
}

export type MovieAddedEvent = {
  type: 'MovieAdded';
  movie: Movie;
}

export type SeriesAddedEvent = {
  type: 'SeriesAdded';
  series: Series;
}

export type ServerEvent =
  | DownloadAddedEvent
  | DownloadProgressEvent
  | DownloadCompletedEvent
  | DownloadRemoved
  | DownloadFailedEvent
  | MovieAddedEvent
  | SeriesAddedEvent;

export type ServerEventHandler = (event: ServerEvent) => void;
export type ServerEventHandlerRegistration = (listener: ServerEventHandler) => (() => void);

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
