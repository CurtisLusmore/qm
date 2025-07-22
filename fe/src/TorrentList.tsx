import {
  useEffect,
  useState,
} from 'react';
import {
  Alert,
  Button,
  ButtonGroup,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  IconButton,
  LinearProgress,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
} from '@mui/material';
import {
  Close,
  Delete,
  Downloading,
  PauseCircle,
} from '@mui/icons-material';
import {
  removeTorrent,
  updateTorrent,
  type Torrent,
} from './Client';
import Util from './Util';

export default function TorrentList(): React.ReactElement {
  const [ loading, setLoading ] = useState(true);
  const [ connectionFailed, setConnectionFailed ] = useState(false);
  const [ torrents, setTorrents ] = useState([] as Torrent[]);
  const [ deleteTorrent, setDeleteTorrent ] = useState(undefined as Torrent | undefined);

  useEffect(function () {
    window.addEventListener('torrents', function (event : Event): void {
      const torrents = (event as CustomEvent).detail as Torrent[];
      setTorrents(torrents);
      setConnectionFailed(false);
      setLoading(false);
    });
    window.addEventListener('failed', () => {
      setConnectionFailed(true);
    });
  }, []);

  return <><Table stickyHeader>
      <TableHead>
        <TableRow>
          <TableCell width='100px'>Progress</TableCell>
          <TableCell width='50px'>Seeders</TableCell>
          <TableCell>Name</TableCell>
          <TableCell width='100px'>State</TableCell>
          <TableCell width='100px'>Actions</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {loading && <TableRow><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell></TableRow>}
        {!loading && connectionFailed && <TableRow><TableCell colSpan={5}><Alert severity='error'>Connection lost. Attempting to reconnect ...</Alert></TableCell></TableRow>}
        {!loading && torrents.length === 0 && <TableRow><TableCell align='center' colSpan={5}>No saved torrents. Press the + button to search for a torrent</TableCell></TableRow>}
        {!loading && torrents.map(ListRow)}
      </TableBody>
    </Table>
    <Dialog open={!!deleteTorrent} onClose={() => setDeleteTorrent(undefined)}>
      <DialogTitle>Delete torrent?</DialogTitle>
      <IconButton
        onClick={() => setDeleteTorrent(undefined)}
        sx={() => ({
          position: 'absolute',
          right: 8,
          top: 8,
        })}
      ><Close /></IconButton>
      <DialogContent>
        <DialogContentText>Are you sure you want to delete {deleteTorrent?.name}?</DialogContentText>
        <DialogContentText>This action cannot be reverted.</DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button autoFocus onClick={() => setDeleteTorrent(undefined)}>Cancel</Button>
        <Button
          color="error"
          onClick={handleDeleteTorrent}
          startIcon={<Delete />}
          variant="contained"
        >Delete</Button>
      </DialogActions>
    </Dialog>
  </>;

  function ListRow(torrent: Torrent): React.ReactElement {
    return <TableRow
      key={torrent.infoHash}
      onClick={() => window.dispatchEvent(new CustomEvent('select', { detail: torrent }))}
      hover
      style={{ cursor: 'pointer' }}
    >
      <TableCell><Progress torrent={torrent} /></TableCell>
      <TableCell align='right'>{torrent.seeders}</TableCell>
      <TableCell><Tooltip title={torrent.infoHash}><span>{torrent.name}</span></Tooltip></TableCell>
      <TableCell>{torrent.state || 'Loading'}</TableCell>
      <TableCell><Actions torrent={torrent} /></TableCell>
    </TableRow>;
  };

  function Progress({ torrent }: { torrent: Torrent }): React.ReactElement {
    const variant = [ 'Stopped', 'Downloading', 'Paused' ].includes(torrent.state)
      ? 'determinate'
      : 'indeterminate';
    const color = torrent.state === 'Paused' ? 'warning' :
      torrent.state === 'Error' ? 'error' :
      torrent.state === 'Stopped' ? 'success' :
        'primary';

    return <Tooltip title={`${Util.FormatBytes(torrent.downloadedBytes)}/${Util.FormatBytes(torrent.sizeBytes)} (${Util.FormatPercent(torrent.progressPercent)})`}>
      <LinearProgress color={color} variant={variant} value={torrent.progressPercent} />
    </Tooltip>
  };

  function Actions({ torrent }: { torrent: Torrent }): React.ReactElement {
    return <ButtonGroup variant='text'>
      {
        [ 'Paused', 'HashingPaused' ].includes(torrent.state)
          ? <Tooltip title='Resume'>
              <Button size='small' onClick={handlePause}><Downloading /></Button>
            </Tooltip>
          : <Tooltip title='Pause'>
              <Button size='small' onClick={handlePause}><PauseCircle /></Button>
            </Tooltip>
      }
      <Tooltip title='Delete'>
        <Button size='small' onClick={handleDelete} color="error"><Delete /></Button>
      </Tooltip>
    </ButtonGroup>;

    async function handlePause(event: React.MouseEvent<HTMLButtonElement>): Promise<void> {
      event.stopPropagation();
      const state = torrent.state === 'Paused'
        ? 'Downloading'
        : 'Paused';
      await updateTorrent(torrent.infoHash, { state });
    };

    function handleDelete(event: React.MouseEvent<HTMLButtonElement>): void {
      event.stopPropagation();
      setDeleteTorrent(torrent);
    };
  };

  async function handleDeleteTorrent(): Promise<void> {
    deleteTorrent && await removeTorrent(deleteTorrent.infoHash);
    setDeleteTorrent(undefined);
  };
};
