import React, {
  useEffect,
  useState,
} from 'react';
import {
  useMediaQuery,
  useTheme,
  Alert,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Collapse,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Divider,
  IconButton,
  LinearProgress,
  ListItemIcon,
  Menu,
  MenuItem,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
  Typography,
  type LinearProgressProps,
  Skeleton,
} from '@mui/material';
import {
  AddCircle,
  Archive,
  CheckCircle,
  Close,
  Delete,
  Downloading,
  Error,
  ExpandLess,
  ExpandMore,
  PauseCircle,
  Pending,
  RemoveCircle,
  DownloadForOffline,
} from '@mui/icons-material';
import {
  removeTorrent,
  updateTorrent,
  type Priority,
  type Torrent,
  type TorrentFile,
} from './Client';
import {
  RefreshRequestEvent,
  RemoveTorrentRequestEvent,
  TorrentsLoadedEvent,
  UpdateTorrentRequestEvent,
} from './Events';
import Util from './Util';

export default function TorrentList(): React.ReactElement {
  const [ loading, setLoading ] = useState(true);
  const [ connectionFailed, setConnectionFailed ] = useState(false);
  const [ torrents, setTorrents ] = useState([] as Torrent[]);
  const [ deleteTorrent, setDeleteTorrent ] = useState(undefined as Torrent | undefined);

  useEffect(() => {
    async function handler(event: Event): Promise<void> {
      const { torrent, confirm } = (event as RemoveTorrentRequestEvent).detail;
      if (confirm) {
        setDeleteTorrent(torrent)
      } else {
        await removeTorrent(torrent.infoHash);
        window.dispatchEvent(new RefreshRequestEvent());
      }
    };
    window.addEventListener('remove', handler);
    return () => window.removeEventListener('remove', handler);
  }, []);

  useEffect(() => {
    async function handler(event: Event): Promise<void> {
      const { infoHash, patch } = (event as UpdateTorrentRequestEvent).detail;
      await updateTorrent(infoHash, patch);
      window.dispatchEvent(new RefreshRequestEvent());
    };
    window.addEventListener('update', handler);
    return () => window.removeEventListener('update', handler);
  }, []);

  useEffect(() => {
    function handler(event : Event): void {
      const { torrents } = (event as TorrentsLoadedEvent).detail;
      torrents.sort((a, b) => a.name.localeCompare(b.name));
      setTorrents(torrents);
      setConnectionFailed(false);
      setLoading(false);
    }
    window.addEventListener('torrents', handler);
    return () => window.removeEventListener('torrents', handler);
  }, []);

  useEffect(() => {
    function handler() {
      setConnectionFailed(true);
      setLoading(false);
    };
    window.addEventListener('failed', handler);
    return () => window.removeEventListener('failed', handler);
  }, []);

  return <>
    {connectionFailed && <Alert severity='error' sx={{ marginBlock: '1em' }}>Connection lost. Attempting to reconnect ...</Alert>}
    {
      loading
        ? <Card>
            <CardContent sx={{ flex: '1' }}>
              <Typography fontSize='large'><Skeleton variant='text' width='20ch' /></Typography>
              <Box sx={{ display: { sm: 'flex' } }} columnGap={2}>
                <Typography sx={{ color: 'text.secondary' }}>
                  <Skeleton variant='text' width='10ch' />
                </Typography>
                <Typography sx={{ color: 'text.secondary' }}>
                  <Skeleton variant='text' width='8ch' />
                </Typography>
                <Typography sx={{ color: 'text.secondary' }}>
                  <Skeleton variant='text' width='15ch' />
                </Typography>
              </Box>
            </CardContent>
            <LinearProgress variant='indeterminate' />
          </Card>
        : torrents.length > 0
        ? <Stack spacing={2}>
            {torrents.map(torrent => <TorrentCard key={torrent.infoHash} torrent={torrent} />)}
          </Stack>
        : <>
            <Typography align='center' variant='h4' padding={4}>
              No saved torrents
            </Typography>
            <Typography align='center'>
              Search for one by clicking the <AddCircle fontSize='small' color='primary' sx={{ marginBottom: '-0.25em' }}/> button
            </Typography>
          </>
    }
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
        <DialogContentText>
          Are you sure you want to delete <span style={{ overflowWrap: 'break-word' }}>{deleteTorrent?.name || deleteTorrent?.infoHash}</span>?
          All downloaded files will be deleted.
        </DialogContentText>
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

  function handleDeleteTorrent(): void {
    window.dispatchEvent(new RemoveTorrentRequestEvent(deleteTorrent!, false));
    setDeleteTorrent(undefined);
  };
};

function TorrentCard({ torrent }: { torrent: Torrent}): React.ReactElement {
  const [ expanded, setExpanded ] = useState(false);

  function handleExpandClicked(): void {
    setExpanded(!expanded);
  };

  return <Card>
    <Box sx={{ display: 'flex' }}>
      <CardContent sx={{ flex: '1', minWidth: 0 }}>
        <Typography fontSize='large' sx={{ overflowWrap: 'break-word' }}>{torrent.name || torrent.infoHash}</Typography>
        <Box sx={{ display: { sm: 'flex' } }} columnGap={2}>
          <Typography sx={{ color: 'text.secondary' }}>
            {stateIcon(torrent)}&nbsp;{torrent.state}
          </Typography>
          <Typography sx={{ color: 'text.secondary' }}>
            {torrent.seeders} seeders
          </Typography>
          <Typography sx={{ color: 'text.secondary' }}>
            {Util.FormatBytes(torrent.downloadedBytes)} of {Util.FormatBytes(torrent.targetBytes)} ({Util.FormatPercent(torrent.partialProgressPercent)})
          </Typography>
        </Box>
      </CardContent>
      <CardActions>
        <Stack direction={{ xs: 'column', sm: 'row' }}>
        {
          [
            ...Actions(torrent),
            <Tooltip key='Expand' title='Expand'>
              <IconButton onClick={handleExpandClicked} disabled={torrent.state === 'Initializing'}>{expanded ? <ExpandLess /> : <ExpandMore />}</IconButton>
            </Tooltip>
          ]
        }
        </Stack>
      </CardActions>
    </Box>
    <LinearProgress
      value={torrent.partialProgressPercent}
      {...progressProps(torrent)}
    />
    <Collapse in={expanded}>
      <CardContent>
        <Stack spacing={1} divider={<Divider />}>
          {torrent.files.map(file => <TorrentFileRow key={file.path} torrent={torrent} file={file} />)}
        </Stack>
      </CardContent>
    </Collapse>
  </Card>;
};

function TorrentFileRow({ torrent, file } : { torrent: Torrent, file: TorrentFile }): React.ReactElement {
  const theme = useTheme();
  const isSmall = useMediaQuery(theme.breakpoints.down('sm'));
  return <Box sx={{ display: 'flex' }} alignItems='center' columnGap={2}>
    <LinearProgress
      value={file.progressPercent}
      color={file.progressPercent == 100.0 ? 'success' : 'primary'}
      variant='determinate'
      sx={{ width: { xs: '25px', sm: '50px' } }}
    />
    <Box sx={{ flex: '1', minWidth: 0, overflowWrap: 'break-word' }}>
      <Typography sx={{ overflowWrap: 'break-word' }}>{file.path}</Typography>
      <Typography fontSize='small' sx={{ color: 'text.secondary' }}>
        {Util.FormatBytes(file.downloadedBytes)} of {Util.FormatBytes(file.sizeBytes)} ({Util.FormatPercent(file.progressPercent)})
      </Typography>
    </Box>
    <Box>
    {
      isSmall
        ? <PriorityMenu torrent={torrent} file={file} />
        : <ToggleButtonGroup
            exclusive
            value={file.priority}
            disabled={file.progressPercent === 100.0}
            onChange={handlePriorityChange}
          >
            <ToggleButton value='Skip'>
              <Tooltip title='Skip'><RemoveCircle /></Tooltip>
            </ToggleButton>
            <ToggleButton value='Normal'>
              <Tooltip title='Normal Priority'><DownloadForOffline /></Tooltip>
            </ToggleButton>
            <ToggleButton value='High'>
              <Tooltip title='High Priority'><Error /></Tooltip>
            </ToggleButton>
          </ToggleButtonGroup>
    }
    </Box>
  </Box>;

  function handlePriorityChange(_: any, priority: Priority): void {
    window.dispatchEvent(new UpdateTorrentRequestEvent(torrent.infoHash, { files: [ { path: file.path, priority } ] }));
  };
};

function PriorityMenu({ torrent, file } : { torrent: Torrent, file: TorrentFile }): React.ReactElement {
  const [ anchorEl, setAnchorEl ] = React.useState(null as Element | null);
  const open = Boolean(anchorEl);
  return <>
    <IconButton onClick={handleOpenClick}>{{Skip: <RemoveCircle />, Normal: <DownloadForOffline />, High: <Error />}[file.priority]}</IconButton>
    <Menu
      anchorEl={anchorEl}
      open={open}
      onClose={handleClose}
      onClick={handleClose}
    >
        <MenuItem onClick={handleItemClick('Skip')}>
          <ListItemIcon><RemoveCircle /></ListItemIcon>
          Skip
        </MenuItem>
        <MenuItem onClick={handleItemClick('Normal')}>
          <ListItemIcon><DownloadForOffline /></ListItemIcon>
          Normal
        </MenuItem>
        <MenuItem onClick={handleItemClick('High')}>
          <ListItemIcon><Error /></ListItemIcon>
          High
        </MenuItem>
    </Menu>
  </>;

  function handleOpenClick (event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
    setAnchorEl(event.currentTarget);
  };

  function handleClose() {
    setAnchorEl(null);
  };

  function handleItemClick(priority: Priority): () => void {
    return function () {
    window.dispatchEvent(new UpdateTorrentRequestEvent(torrent.infoHash, { files: [ { path: file.path, priority } ] }));
    };
  };
};

function progressProps(torrent: Torrent) : LinearProgressProps {
  switch (torrent.state) {
    case 'Initializing': return {
      color: 'info',
      variant: 'indeterminate',
    };
    case 'Downloading': return {
      color: 'primary',
      variant: 'buffer',
      valueBuffer: torrent.targetPercent,
    };
    case 'Paused': return {
      color: 'warning',
      variant: 'buffer',
      valueBuffer: torrent.targetPercent,
    };
    case 'Complete': return {
      color: 'success',
      variant: 'determinate',
    };
    case 'Error': return {
      color: 'error',
      variant: 'indeterminate',
    };
    default: return {};
  }
};

function stateIcon(torrent: Torrent): React.ReactElement {
  switch (torrent.state) {
    case 'Initializing': return <Pending fontSize='small' sx={{ marginBottom: '-0.2em' }} color='info' />;
    case 'Downloading': return <Downloading fontSize='small' sx={{ marginBottom: '-0.2em' }} color='primary' />;
    case 'Paused': return <PauseCircle fontSize='small' sx={{ marginBottom: '-0.2em' }} color='warning' />;
    case 'Complete': return <CheckCircle fontSize='small' sx={{ marginBottom: '-0.2em' }} color='success' />;
    case 'Error': return <Error fontSize='small' sx={{ marginBottom: '-0.2em' }} color='error' />;
    default: return <></>;
  }
};

function Actions(torrent: Torrent): React.ReactElement[] {
  switch (torrent.state) {
    case 'Initializing':
      return [
        <Tooltip key='Delete' title='Delete'>
          <IconButton onClick={handleDeleteClick} color='error'><Delete /></IconButton>
        </Tooltip>,
      ];

    case 'Error':
      return [
        <Tooltip key='Delete' title='Delete'>
          <IconButton onClick={handleDeleteClick} color='error'><Delete /></IconButton>
        </Tooltip>,
      ];

    case 'Downloading':
      return [
        <Tooltip key='Pause' title='Pause'>
          <IconButton onClick={handlePauseClick} color='primary'><PauseCircle /></IconButton>
        </Tooltip>,
        <Tooltip key='Delete' title='Delete'>
          <IconButton onClick={handleDeleteClick} color='error'><Delete /></IconButton>
        </Tooltip>,
      ];

    case 'Paused':
      return [
        <Tooltip key='Resume' title='Resume'>
          <IconButton onClick={handleResumeClick} color='primary'><Downloading /></IconButton>
        </Tooltip>,
        <Tooltip key='Delete' title='Delete'>
          <IconButton onClick={handleDeleteClick} color='error'><Delete /></IconButton>
        </Tooltip>,
      ];

    case 'Complete':
      return [
        <Tooltip key='Archive' title='Archive'>
          <IconButton onClick={handleArchiveClick} color='success'><Archive /></IconButton>
        </Tooltip>,
      ];
    default: return [];
  }

  function handleArchiveClick() {
    window.dispatchEvent(new RemoveTorrentRequestEvent(torrent, false));
  };

  function handleDeleteClick() {
    window.dispatchEvent(new RemoveTorrentRequestEvent(torrent, true));
  };

  function handlePauseClick() {
    window.dispatchEvent(new UpdateTorrentRequestEvent(torrent.infoHash, { state: 'Paused' }));
  };

  function handleResumeClick() {
    window.dispatchEvent(new UpdateTorrentRequestEvent(torrent.infoHash, { state: 'Downloading' }));
  };
};
