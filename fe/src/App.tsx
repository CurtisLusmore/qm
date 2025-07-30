import {
  useEffect,
} from 'react';
import {
  Container,
} from '@mui/material';
import {
  getTorrents,
} from './Client';
import {
  TorrentsFailedEvent,
  TorrentsLoadedEvent,
} from './Events';
import SearchForm from './SearchForm';
import TorrentList from './TorrentList';

const loadTorrents = (function (): ((force: boolean) => Promise<void>) {
  let inProgress = false;
  return async function (force: boolean = false): Promise<void> {
    if (!force && inProgress) return;
    inProgress = true;
    try {
      const torrents = await getTorrents();
      window.dispatchEvent(new TorrentsLoadedEvent(torrents));
    }
    catch {
      window.dispatchEvent(new TorrentsFailedEvent());
    }
    finally {
      inProgress = false;
    }
  };
}());

export default function App() {
  useEffect(() => {
    const handle = setInterval(loadTorrents, 1000);
    return () => clearInterval(handle);
  }, []);

  useEffect(() => {
    function handler(event: Event) {
      const { torrents } = (event as TorrentsLoadedEvent).detail;
      const complete = torrents.filter(torrent => torrent.state === 'Complete').length;
      const total = torrents.length;
      document.title = total === 0
        ? 'Quartermaster'
        : `Quartermaster (${complete}/${total})`;
    };
    window.addEventListener('torrents', handler);
    return () => window.removeEventListener('torrents', handler);
  }, []);

  useEffect(() => {
    function handler() {
      loadTorrents(true);
    };
    window.addEventListener('refresh', handler);
    return () => window.removeEventListener('refresh', handler);
  }, []);

  return <Container maxWidth='lg' sx={{ padding: 0 }}>
    <SearchForm />
    <TorrentList />
  </Container>;
};
