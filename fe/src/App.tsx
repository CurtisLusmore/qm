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

const loadTorrents = (function () {
  let inProgress = false;
  return async function () {
    if (inProgress) return;
    inProgress = true;
    try {
      const torrents = await getTorrents();
      window.dispatchEvent(new CustomEvent('torrents', { detail: torrents }));
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

  return <Container maxWidth='lg'>
    <SearchForm />
    <TorrentList />
    <TorrentFileList />
  </Container>;
};
