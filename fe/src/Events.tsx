import {
  type Torrent,
  type TorrentPatch,
} from './Client';

export class RefreshRequestEvent extends CustomEvent<void> {
  constructor() {
    super('remove', { detail: undefined });
  }
};

export class RemoveTorrentRequestEvent extends CustomEvent<RemoveTorrentRequest> {
  constructor(torrent: Torrent, confirm: Boolean) {
    super('remove', { detail: { torrent, confirm } });
  }
};

type RemoveTorrentRequest = {
  torrent: Torrent,
  confirm: Boolean,
};

export class TorrentsFailedEvent extends CustomEvent<void> {
  constructor() {
    super('failed', { detail: undefined });
  }
};

export class TorrentsLoadedEvent extends CustomEvent<TorrentsLoaded> {
  constructor(torrents: Torrent[]) {
    super('torrents', { detail: { torrents }});
  }
};

type TorrentsLoaded = {
  torrents: Torrent[],
};

export class UpdateTorrentRequestEvent extends CustomEvent<UpdateTorrentRequest> {
  constructor(infoHash: string, patch: TorrentPatch) {
    super('update', { detail: { infoHash, patch } });
  }
};

type UpdateTorrentRequest = {
  infoHash: string,
  patch: TorrentPatch,
};
