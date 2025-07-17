import {
  useEffect,
  useState,
} from 'react';

import {
  searchTorrents,
  getTorrents,
  type Torrent,
  type TorrentSearchResult,
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
  const [ loading, setLoading ] = useState(false);
  const [ results, setResults ] = useState([] as TorrentSearchResult[]);

  const handleSearchInput = (function () {
    let handle: number | undefined = undefined;
    return async function handleSearchInput(event: React.FormEvent<HTMLInputElement>): Promise<void> {
      clearTimeout(handle);
      handle = setTimeout(async function () {
        setLoading(true);
        try {
          const results = await searchTorrents((event.target as HTMLInputElement).value);
          setResults(results || []);
        }
        finally {
          setLoading(false);
        }
      }, 200);
    };
  }());

  return <>
    <form>
      <input type="search" onInput={handleSearchInput} />
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
    return <tr key={result.infoHash}>
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

  return <table>
    <thead>
      <tr>
        <th>Progress</th>
        <th>Seeders</th>
        <th>Name</th>
        <th>State</th>
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
    </tr>;
  };
};
