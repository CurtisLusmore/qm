import {
  useEffect,
  useMemo,
  useState,
} from 'react';

import {
  searchTorrents,
  getTorrents,
  removeTorrent,
  saveTorrent,
  updateTorrent,
  type Torrent,
  type TorrentSearchResult,
  type State,
} from './Client';

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

  return <>
    <SearchForm />
    <TorrentList />
  </>;
};

function SearchForm() {
  const [ terms, setTerms ] = useState('');
  const [ loading, setLoading ] = useState(false);
  const [ results, setResults ] = useState([] as TorrentSearchResult[]);

  const handleSearchInput = useMemo(function () {
    let handle: number | undefined = undefined;
    return async function handleSearchInput(event: React.FormEvent<HTMLInputElement>): Promise<void> {
      const terms = (event.target as HTMLInputElement).value
      setTerms(terms);

      clearTimeout(handle);
      handle = setTimeout(async function () {
        setLoading(true);
        try {
          const results = await searchTorrents(terms);
          setResults(results || []);
        }
        finally {
          setLoading(false);
        }
      }, 200);
    };
  }, []);

  const createClickHandler = function (infoHash: string): React.MouseEventHandler {
    return async function (): Promise<void> {
      setTerms('');
      setResults([]);
      await saveTorrent(infoHash);
    };
  };

  return <>
    <form>
      <input type="search" onInput={handleSearchInput} value={terms} />
    </form>
    {
      loading
        ? <></>
        : <table>
          <thead>
            <tr>
              <th>Size</th>
              <th>Seeders</th>
              <th>Name</th>
            </tr>
          </thead>
          <tbody>{results.map(Row)}</tbody>
        </table>
    }
  </>;

  function Row(result: TorrentSearchResult): React.ReactNode {
    return <tr
        key={result.infoHash}
        onClick={createClickHandler(result.infoHash)}
        style={{ cursor: 'pointer' }}
      >
      <td>{result.sizeBytes}</td>
      <td>{result.seeders}</td>
      <td>{result.name}</td>
    </tr>;
  }
};

function TorrentList() {
  const [ torrents, setTorrents ] = useState([] as Torrent[]);
  useEffect(() => {
    window.addEventListener('torrents', (event : Event) => {
      const torrents = (event as CustomEvent).detail as Torrent[];
      setTorrents(torrents);
    });
  }, []);

  const createRemoveClickHandler = function (infoHash: string) {
    return async function (): Promise<void> {
      await removeTorrent(infoHash);
    };
  };

  const createStateClickHandler = function (infoHash: string, state: State) {
    return async function (): Promise<void> {
      await updateTorrent(infoHash, { state });
    };
  };

  return <table>
    <thead>
      <tr>
        <th>Progress</th>
        <th>Seeders</th>
        <th>Name</th>
        <th>State</th>
        <th>Action</th>
      </tr>
    </thead>
    <tbody>{ torrents.map(Row) }</tbody>
  </table>;

  function Row(torrent: Torrent): React.ReactNode {
    return <tr key={torrent.infoHash}>
      <td>{torrent.progressPercent}%</td>
      <td>{torrent.seeders}</td>
      <td>{torrent.name}</td>
      <td>{torrent.state}</td>
      <td>
        <button onClick={createRemoveClickHandler(torrent.infoHash)}>Remove</button>
        <button onClick={createStateClickHandler(torrent.infoHash, 'Paused')}>Pause</button>
        <button onClick={createStateClickHandler(torrent.infoHash, 'Downloading')}>Resume</button>
      </td>
    </tr>;
  };
};
