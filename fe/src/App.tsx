import {
  useEffect,
} from 'react';
import {
  Container,
} from '@mui/material';
import {
  getTorrents,
} from './Client';
import SearchForm from './SearchForm';
import TorrentList from './TorrentList';
import TorrentFileList from './TorrentFileList';

const loadTorrents = (function (): ((force: boolean) => Promise<void>) {
  let inProgress = false;
  return async function (force: boolean = false): Promise<void> {
    if (!force && inProgress) return;
    inProgress = true;
    try {
      const torrents = await getTorrents();
      window.dispatchEvent(new CustomEvent('torrents', { detail: torrents }));
    }
    catch {
      window.dispatchEvent(new CustomEvent('failed'));
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
    function handler() {
      loadTorrents(true);
    };
    window.addEventListener('refresh', handler);
    return () => window.removeEventListener('refresh', handler);
  }, []);

  return <Container maxWidth='lg'>
    <SearchForm />
    <TorrentList />
    <TorrentFileList />
  </Container>;
};
