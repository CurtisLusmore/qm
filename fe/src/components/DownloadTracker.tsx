import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Fab,
  LinearProgress,
  Typography,
  Stack,
  type LinearProgressProps,
  type SvgIconProps,
} from '@mui/material';
import {
  CheckCircle,
  Downloading,
  Error,
  PauseCircle,
  Pending,
} from '@mui/icons-material';
import { useCollection } from '../hooks';
import type { DownloadTracker, DownloadTrackerStatus } from '../types';

export default function DownloadTracker(): React.ReactElement {
  const [ open, setOpen ] = useState(false);
  const { downloads } = useCollection();

  return downloads.length === 0 ? <></> : (
    <>
      <FloatingButton downloads={downloads} onClick={() => setOpen(true)} />
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="md">
        <DialogTitle>Downloads</DialogTitle>
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
        <DialogActions>
          <Button onClick={() => setOpen(false)} color="primary">
            Close
          </Button>
        </DialogActions>
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
  return (
    <Card>
      <Box sx={{ display: 'flex'}}>
        <CardContent sx={{ flexGrow: 1 }}>
          <Typography variant="h6">{download.title.name} ({download.title.year})</Typography>
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
      </Box>
      <LinearProgress {...useProgressProps(download)} />
    </Card>
  );
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
      return <Typography color="textSecondary"><Pending {...props} color="info" />&nbsp;Initializing</Typography>;
    case 'DownloadingTorrent':
      return <Typography color="textSecondary"><Downloading {...props} color="primary" />&nbsp;Downloading</Typography>;
    case 'PausedTorrent':
      return <Typography color="textSecondary"><PauseCircle {...props} color="warning" />&nbsp;Paused</Typography>;
    case 'DownloadedTorrent':
    case 'StoppingTorrent':
    case 'StoppedTorrent':
      return <Typography color="textSecondary"><CheckCircle {...props} color="success" />&nbsp;Completed</Typography>;
    case 'DownloadTorrentFailed':
      return <Typography color="textSecondary"><Error {...props} color="error" />&nbsp;Failed</Typography>;
    default:
      return <Typography color="textSecondary">{status}</Typography>;
  }
};

function formatBytes(bytes: number): string {
  if (bytes === 0) return '0B';
  const k = 1024;
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
      return { variant: 'query', color: 'info' };
    case 'DownloadingTorrent':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent };
    case 'PausedTorrent':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent, color: 'warning' };
    case 'DownloadedTorrent':
    case 'StoppingTorrent':
    case 'StoppedTorrent':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent, color: 'success' };
    case 'DownloadTorrentFailed':
      return { variant: 'indeterminate', color: 'error' };
    default:
      return {
        variant: 'indeterminate',
      };
  }
};
