import React, {
  useEffect,
  useState,
} from 'react';
import {
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
  LinearProgress,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
} from '@mui/material';
import {
  CheckCircle,
  Close,
  Error,
  RemoveCircle,
} from '@mui/icons-material';
import {
  updateTorrent,
  type Priority,
  type Torrent,
  type TorrentFile,
} from './Client';
import Util from './Util';

export default function TorrentFileList(): React.ReactElement {
  const [ infoHash, setInfoHash ] = useState('');
  const open = infoHash !== '';
  const [ torrent, setTorrent ] = useState(undefined as Torrent | undefined);

  useEffect(() => {
    window.addEventListener('select', function (event: Event): void {
      const torrent = (event as CustomEvent).detail as Torrent;
      setInfoHash(torrent.infoHash);
      setTorrent(torrent);
    });
  }, []);

  useEffect(() => {
    const listener = function (event: Event): void {
      const torrents = (event as CustomEvent).detail as Torrent[];
      const torrent = torrents.find(torrent => torrent.infoHash === infoHash);
      if (torrent !== undefined) {
        setTorrent(torrent);
      } else {
        setInfoHash('');
      }
    };
    window.addEventListener('torrents', listener);
    return () => window.removeEventListener('torrents', listener);
  }, [ infoHash ]);

  return <Dialog
    open={open}
    fullWidth
    maxWidth='lg'
    onClose={() => setInfoHash('')}
  >
    <DialogTitle>{torrent?.name}</DialogTitle>
    <IconButton
      onClick={() => setInfoHash('')}
      sx={{
        position: 'absolute',
        right: 8,
        top: 8,
      }}
    ><Close /></IconButton>
    <DialogContent sx={{ paddingBlock: 0 }}>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell width='100px'>Progress</TableCell>
            <TableCell>Path</TableCell>
            <TableCell width='50px'>Priority</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(torrent?.files.length ?? 0) > 0
            ? torrent!.files.map(ListRow)
            : <TableRow><TableCell align='center' colSpan={3}>Loading files...</TableCell></TableRow>}
        </TableBody>
      </Table>
    </DialogContent>
  </Dialog>;

  function ListRow(file: TorrentFile): React.ReactElement {
    return <TableRow key={file.path}>
      <TableCell><Progress file={file} /></TableCell>
      <TableCell>{file.path}</TableCell>
      <TableCell>
        <ToggleButtonGroup
          exclusive
          value={file.priority}
          onChange={handlePriority}
        >
          <ToggleButton value='Skip'>
            <Tooltip title='Skip'><RemoveCircle /></Tooltip>
          </ToggleButton>
          <ToggleButton value='Normal'>
            <Tooltip title='Normal'><CheckCircle /></Tooltip>
          </ToggleButton>
          <ToggleButton value='High'>
            <Tooltip title='High'><Error /></Tooltip>
          </ToggleButton>
        </ToggleButtonGroup>
      </TableCell>
    </TableRow>;

    async function handlePriority(_: any, value: Priority): Promise<void> {
      await updateTorrent(infoHash, { files: [ { path: file.path, priority: value } ] });
      window.dispatchEvent(new CustomEvent('refresh'));
    };
  };
};

function Progress({ file }: { file: TorrentFile }): React.ReactElement {
  const color = file.progressPercent < 100 ? 'primary' : 'success';

  return <Tooltip title={`${Util.FormatBytes(file.downloadedBytes)}/${Util.FormatBytes(file.sizeBytes)} (${Util.FormatPercent(file.progressPercent)})`}>
    <LinearProgress color={color} variant='determinate' value={file.progressPercent} />
  </Tooltip>
};
