import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  CircularProgress,
  Collapse,
  Dialog,
  DialogTitle,
  DialogContent,
  Fab,
  IconButton,
  LinearProgress,
  type LinearProgressProps,
  Stack,
  type SvgIconProps,
  Tooltip,
  Typography,
} from '@mui/material';
import {
  CheckCircle,
  Close,
  DownloadForOffline,
  Downloading,
  DriveFileMove,
  Error,
  ExpandLess,
  ExpandMore,
  PauseCircle,
  Pending,
} from '@mui/icons-material';
import { useCollection, useWakeLock } from '../hooks';
import type { DownloadTracker, DownloadTrackerStatus, FileTracker } from '../types';

export default function DownloadTracker(): React.ReactElement {
  const [ open, setOpen ] = useState(false);
  const { downloads } = useCollection();
  const { isLocked, requestWakeLock, releaseWakeLock } = useWakeLock();

  function handleClickOpen() {
    setOpen(true);
    requestWakeLock();
  }

  function handleClose() {
    setOpen(false);
    releaseWakeLock();
  }

  return downloads.length === 0 ? <></> : (
    <>
      <FloatingButton downloads={downloads} onClick={handleClickOpen} />
      <Dialog 
        open={open}
        onClose={handleClose}
        fullWidth
        maxWidth="md"
        sx={{
          '& .MuiDialog-paper': {
            bgcolor: 'background.default',
          },
        }}
      >
        <Button
          sx={{
            position: 'fixed',
            bottom: 16,
            left: '50%',
            transform: 'translateX(-50%)',
            zIndex: 1000,
            borderRadius: '20px',
          }}
          variant="contained"
          onClick={isLocked ? releaseWakeLock : requestWakeLock}
        >
          {isLocked ? 'Release Wake Lock' : 'Request Wake Lock'}
        </Button>
        <DialogTitle>
          Downloads
          <IconButton onClick={handleClose} color="inherit" sx={{ position: 'absolute', right: 8, top: 8 }}>
            <Close />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          {downloads.length === 0 ? (
            <p>No active downloads.</p>
          ) : (
            <Stack spacing={2}>
              {downloads.map(download => (
                <DownloadCard
                  key={download.infoHash}
                  download={download}
                />
              ))}
            </Stack>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
};

function FloatingButton({ downloads, onClick }: { downloads: DownloadTracker[], onClick: () => void }): React.ReactElement {
  const totalDownloadedBytes = downloads.reduce((sum, download) => sum + download.downloadedBytes, 0);
  const totalTargetBytes = downloads.reduce((sum, download) => sum + download.targetBytes, 0);
  const overallProgressPercent = totalTargetBytes > 0 ? (totalDownloadedBytes / totalTargetBytes) * 100 : 0;
  const anyFailed = downloads.some(download => download.status === 'DownloadTorrentFailed');
  const allCompleted = downloads.every(download => ['DownloadedTorrent', 'StoppedTorrent', 'StoppingTorrent'].includes(download.status));

  return (
    <Fab
      onClick={onClick}
      color={anyFailed ? 'error' : allCompleted ? 'success' : 'primary'}
      style={{ position: 'fixed', bottom: 16, right: 16 }}
      title={`${downloads.length} active download${downloads.length > 1 ? 's' : ''}, overall progress: ${overallProgressPercent.toFixed(1)}%`}
    >
      <CircularProgress
        variant="determinate"
        value={overallProgressPercent}
        size={68}
        sx={{
          color: anyFailed ? 'error.main' : allCompleted ? 'success.main' : 'primary.main',
          position: 'absolute',
          top: -6,
          left: -6,
          zIndex: 1,
        }}
      />
      {anyFailed
        ? <Error sx={{ position: 'absolute', fontSize: '2.5em' }} />
        : allCompleted
          ? <CheckCircle sx={{ position: 'absolute', fontSize: '2.5em' }} />
          : <Downloading sx={{ position: 'absolute', fontSize: '2.5em' }} />
      }
    </Fab>
  );
};

function DownloadCard({ download }: { download: DownloadTracker }): React.ReactElement {
  const [ expanded, setExpanded ] = useState(false);
  function handleClickExpand() {
    setExpanded(!expanded);
  };

  let name: string;
  switch (download.title.type.toLowerCase()) {
    case 'movie':
      name = `${download.title.name} (${download.title.year})`;
      break;
    case 'series':
      name = `${download.title.name} (${download.title.year}): ${getEpisodes(download)}`;
      break;
    default:
      name = `${download.title.name} (${download.title.year})`;
  }

  return (
    <Card>
      <Box sx={{ display: 'flex'}}>
        <CardContent sx={{ flexGrow: 1 }}>
          <Typography variant="h6">{name}</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, justifyContent: 'flex-start', padding: 0 }}>
            {formatStatus(download.status)}
            <Typography color="textSecondary">{download.seeds} seeders</Typography>
            <Typography color="textSecondary">{formatBytes(download.downloadedBytes)} / {formatBytes(download.targetBytes)} ({formatPercent(download.partialProgressPercent)})</Typography>
            <Typography color="textSecondary">{formatBytes(download.bytesPerSecond)}ps</Typography>
          </Box>
          {download.error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {download.error}
            </Alert>
          )}
        </CardContent>
        <CardActions>
          <Tooltip key="Expand" title={expanded ? 'Collapse' : 'Expand'}>
            <IconButton onClick={handleClickExpand} disabled={!download.files || download.files.length === 0}>
              {expanded ? <ExpandLess /> : <ExpandMore />}
            </IconButton>
          </Tooltip>
        </CardActions>
      </Box>
      <LinearProgress {...useProgressProps(download)} />
      <Collapse in={expanded}>
        <Stack spacing={1}>
          {download.files?.map(file => (
            <DownloadFileRow key={file.path} file={file} />
          ))}
        </Stack>
      </Collapse>
    </Card>
  );
};

function DownloadFileRow({ file }: { file: FileTracker }): React.ReactElement {
  return (
    <Box>
      <Box sx={{ flexGrow: 1, minWidth: 0, overflowWrap: 'break-word', p: 2 }}>
        <Typography sx={{ overflowWrap: 'break-word' }}>{file.path}</Typography>
        <Typography color="textSecondary">
          {formatBytes(file.downloadedBytes)} / {formatBytes(file.totalBytes)} ({formatPercent(file.progressPercent)})
        </Typography>
      </Box>
      <LinearProgress
        value={file.progressPercent}
        color={file.progressPercent === 100 ? 'success' : 'primary'}
        variant="determinate"
      />
    </Box>
  );
};

function getEpisodes(download: DownloadTracker): string {
  if (!download.files || download.files.length === 0) {
    return '';
  }

  const eps = new Map<number, Set<number>>();
  for (const file of download.files) {
    const match = file.path.match(/[Ss](\d+)[Ee](\d+)/i);
    if (match) {
      const season = parseInt(match[1], 10);
      const episode = parseInt(match[2], 10);
      if (!eps.has(season)) {
        eps.set(season, new Set());
      }
      eps.get(season)!.add(episode);
    }
  }
  if (eps.size !== 1) {
    return '';
  } else {
    const season = Array.from(eps.keys())[0];
    if (eps.get(season)!.size === 1) {
      const episode = Array.from(eps.get(season)!)[0];
      return `S${leftPad(season.toString(), 2, '0')}E${leftPad(episode.toString(), 2, '0')}`;
    } else {
      return `S${leftPad(season.toString(), 2, '0')}`;
    }
  }
};

function formatStatus(status: DownloadTrackerStatus): React.ReactElement {
  const props: SvgIconProps = {
    fontSize: 'small',
    sx: { mb: '-0.2em' },
  };
  switch (status) {
    case 'Received':
    case 'DownloadingTorrentFile':
    case 'DownloadTorrentFileFailed':
    case 'DownloadedTorrentFile':
    case 'AddedTorrent':
    case 'StartedTorrent':
    case 'InitializingTorrent':
      return <Typography color="textSecondary"><Pending {...props} color="primary" />&nbsp;Initializing</Typography>;
    case 'DownloadingTorrent':
      return <Typography color="textSecondary"><Downloading {...props} color="primary" />&nbsp;Downloading</Typography>;
    case 'PausedTorrent':
      return <Typography color="textSecondary"><PauseCircle {...props} color="warning" />&nbsp;Paused</Typography>;
    case 'StoppingTorrent':
      return <Typography color="textSecondary"><CheckCircle {...props} color="success" />&nbsp;Completing</Typography>;
    case 'DownloadTorrentFailed':
      return <Typography color="textSecondary"><Error {...props} color="error" />&nbsp;Failed</Typography>;
    case 'DownloadedTorrent':
      return <Typography color="textSecondary"><DownloadForOffline {...props} color="success" />&nbsp;Downloaded</Typography>;
    case 'SortingFiles':
      return <Typography color="textSecondary"><DriveFileMove {...props} color="success" />&nbsp;Sorting</Typography>;
    case 'ManualSortingRequired':
      return <Typography color="textSecondary"><DriveFileMove {...props} color="warning" />&nbsp;Manual Sorting Required</Typography>;
    case 'Completed':
      return <Typography color="textSecondary"><CheckCircle {...props} color="success" />&nbsp;Completed</Typography>;
    default:
      return <Typography color="textSecondary">{status}</Typography>;
  }
};

function formatBytes(bytes: number): string {
  if (bytes === 0) return '0B';
  const k = 1000;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + sizes[i];
};

function formatPercent(percent: number): string {
  return `${percent.toFixed(1)}%`;
};

function useProgressProps(download: DownloadTracker): LinearProgressProps {
  switch (download.status) {
    case 'Received':
    case 'DownloadingTorrentFile':
    case 'DownloadTorrentFileFailed':
    case 'DownloadedTorrentFile':
    case 'AddedTorrent':
    case 'StartedTorrent':
    case 'InitializingTorrent':
      return { variant: 'query', color: 'primary' };
    case 'DownloadingTorrent':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent };
    case 'PausedTorrent':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent, color: 'warning' };
    case 'DownloadedTorrent':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent, color: 'success' };
    case 'StoppingTorrent':
      return { variant: 'indeterminate', color: 'success' };
    case 'DownloadedTorrent':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent, color: 'success' };
    case 'DownloadTorrentFailed':
      return { variant: 'indeterminate', color: 'error' };
    case 'SortingFiles':
      return { variant: 'indeterminate', color: 'success' };
    case 'ManualSortingRequired':
      return { variant: 'query', color: 'warning' };
    case 'Completed':
      return { variant: 'buffer', value: 100, valueBuffer: 100, color: 'success' };
    default:
      return {
        variant: 'indeterminate',
      };
  }
};

function leftPad(str: string, length: number, padChar: string): string {
  while (str.length < length) {
    str = padChar + str;
  }
  return str;
};
