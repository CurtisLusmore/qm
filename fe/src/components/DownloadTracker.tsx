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
import { useDownloadTrackers } from '../clients';
import type { DownloadTracker, DownloadTrackerStatus } from '../types';

export default function DownloadTracker(): React.ReactElement {
  const trackers = useDownloadTrackers();
  const [ open, setOpen ] = useState(false);

  return trackers.length === 0 ? <></> : (
    <>
      <FloatingButton trackers={trackers} onClick={() => setOpen(true)} />
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="md">
        <DialogTitle>Download Trackers</DialogTitle>
        <DialogContent>
          {trackers.length === 0 ? (
            <p>No active downloads.</p>
          ) : (
            <Stack spacing={2}>
              {trackers.map((tracker) => (
                <DownloadCard
                  key={tracker.infoHash}
                  tracker={tracker}
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

function FloatingButton({ trackers, onClick }: { trackers: DownloadTracker[], onClick: () => void }): React.ReactElement {
  const totalDownloadedBytes = trackers.reduce((sum, tracker) => sum + tracker.downloadedBytes, 0);
  const totalTargetBytes = trackers.reduce((sum, tracker) => sum + tracker.targetBytes, 0);
  const overallProgressPercent = totalTargetBytes > 0 ? (totalDownloadedBytes / totalTargetBytes) * 100 : 0;
  const anyFailed = trackers.some(tracker => tracker.status === 'DownloadTorrentFailed');
  const allCompleted = trackers.every(tracker => ['DownloadedTorrent', 'StoppedTorrent', 'StoppingTorrent'].includes(tracker.status));

  return (
    <Fab
      onClick={onClick}
      color={anyFailed ? 'error' : allCompleted ? 'success' : 'primary'}
      style={{ position: 'fixed', bottom: 16, right: 16 }}
      title={`${trackers.length} active download${trackers.length > 1 ? 's' : ''}, overall progress: ${overallProgressPercent.toFixed(1)}%`}
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

function DownloadCard({ tracker }: { tracker: DownloadTracker }): React.ReactElement {
  return (
    <Card>
      <Box sx={{ display: 'flex'}}>
        <CardContent sx={{ flexGrow: 1 }}>
          <Typography variant="h6">{tracker.title.name} ({tracker.title.year})</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, justifyContent: 'flex-start', padding: 0 }}>
            {formatStatus(tracker.status)}
            <Typography color="textSecondary">{tracker.seeds} seeders</Typography>
            <Typography color="textSecondary">{formatBytes(tracker.downloadedBytes)} / {formatBytes(tracker.targetBytes)} ({formatPercent(tracker.partialProgressPercent)})</Typography>
            <Typography color="textSecondary">{formatBytes(tracker.bytesPerSecond)}ps</Typography>
          </Box>
          {tracker.error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {tracker.error}
            </Alert>
          )}
        </CardContent>
      </Box>
      <LinearProgress {...useProgressProps(tracker)} />
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

function useProgressProps(tracker: DownloadTracker): LinearProgressProps {
  switch (tracker.status) {
    case 'Received':
    case 'DownloadingTorrentFile':
    case 'DownloadTorrentFileFailed':
    case 'DownloadedTorrentFile':
    case 'AddedTorrent':
    case 'StartedTorrent':
    case 'InitializingTorrent':
      return { variant: 'query', color: 'info' };
    case 'DownloadingTorrent':
      return { variant: 'buffer', value: tracker.partialProgressPercent, valueBuffer: tracker.targetProgressPercent };
    case 'PausedTorrent':
      return { variant: 'buffer', value: tracker.partialProgressPercent, valueBuffer: tracker.targetProgressPercent, color: 'warning' };
    case 'DownloadedTorrent':
    case 'StoppingTorrent':
    case 'StoppedTorrent':
      return { variant: 'buffer', value: tracker.partialProgressPercent, valueBuffer: tracker.targetProgressPercent, color: 'success' };
    case 'DownloadTorrentFailed':
      return { variant: 'indeterminate', color: 'error' };
    default:
      return {
        variant: 'indeterminate',
      };
  }
};
