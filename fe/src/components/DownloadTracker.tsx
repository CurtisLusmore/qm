import { useEffect, useState } from 'react';
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
  DialogActions,
} from '@mui/material';
import {
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
} from '@mui/icons-material';
import { pauseDownload, resumeDownload, removeDownload } from '../clients';
import {
  useDispatchToast,
  useDownloads,
  useServerEventHandlerRegistration,
  useWakeLock,
} from '../hooks';
import type { DownloadTracker, DownloadTrackerStatus, FileTracker } from '../types';

export default function DownloadTracker(): React.ReactElement {
  const [ open, setOpen ] = useState(false);
  const downloads = useDownloads();
  const { isLocked, requestWakeLock, releaseWakeLock } = useWakeLock();
  const registration = useServerEventHandlerRegistration();
  const dispatchToast = useDispatchToast();

  useEffect(() => {
    return registration(function handleServerEvent(event) {
      switch (event.type) {
        case 'DownloadAdded':
          dispatchToast(`Download added: ${event.download.name}`);
          break;

        case 'DownloadCompleted':
          dispatchToast(`Download completed: ${event.download.name}`, 'success');
          break;

        case 'DownloadFailed':
          dispatchToast(`Download failed: ${event.download.name}`, 'error');
          break;
      }
    });
  }, [ registration, dispatchToast ]);

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
  const anyFailed = downloads.some(download => download.status === 'Failed');
  const allCompleted = downloads.every(download => download.status === 'Completed');

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
  const [ deleteDialogOpen, setDeleteDialogOpen ] = useState(false);

  function handleClickExpand() {
    setExpanded(!expanded);
  };

  function onPause() {
    pauseDownload(download.infoHash);
  };

  function onResume() {
    resumeDownload(download.infoHash);
  };

  function onDelete() {
    setDeleteDialogOpen(true);
  };

  async function handleConfirmDelete() {
    await removeDownload(download.infoHash);
  };

  return (
    <>
      <Card>
        <Box sx={{ display: 'flex'}}>
          <CardContent sx={{ flexGrow: 1 }}>
            <Typography variant="h6">{download.name}</Typography>
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
            <Stack direction="row">
              {[
                ...DownloadActions({ download, onPause, onResume, onDelete }),
                <Tooltip key="Expand" title={expanded ? 'Collapse' : 'Expand'}>
                  <IconButton onClick={handleClickExpand} disabled={!download.files || download.files.length === 0}>
                    {expanded ? <ExpandLess /> : <ExpandMore />}
                  </IconButton>
                </Tooltip>
              ]}
            </Stack>
          </CardActions>
        </Box>
        <LinearProgress {...useProgressProps(download)} />
        <Collapse in={expanded}>
          <Stack spacing={1}>
            {download.files?.filter(file => file.priority !== 'Skip').map(file => (
              <DownloadFileRow key={file.path} file={file} />
            ))}
          </Stack>
        </Collapse>
      </Card>
      <ComfirmDeleteDialog
        tracker={download}
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
        onConfirm={handleConfirmDelete}
      />
    </>
  );
};

function DownloadActions({ download, onPause, onResume, onDelete }: {
  download: DownloadTracker,
  onPause: () => void,
  onResume: () => void,
  onDelete: () => void,
}): React.ReactElement[] {
  const deleteAction = (
    <Tooltip key="Delete" title="Delete">
      <IconButton color="error" onClick={onDelete}>
        <Delete />
      </IconButton>
    </Tooltip>
  );

  const pauseAction = (
    <Tooltip key="Pause" title="Pause">
      <IconButton onClick={onPause}>
        <PauseCircle />
      </IconButton>
    </Tooltip>
  );

  const resumeAction = (
    <Tooltip key="Resume" title="Resume">
      <IconButton onClick={onResume}>
        <Downloading />
      </IconButton>
    </Tooltip>
  );

  const archiveAction = (
    <Tooltip key="Archive" title="Archive">
      <IconButton color="success" onClick={() => { /* TODO: Implement archive action */ }}>
        <Archive />
      </IconButton>
    </Tooltip>
  );

  switch (download.status) {
    case 'Received':
    case 'DownloadingTorrentFile':
    case 'AddingTorrent':
    case 'MappingFiles':
    case 'LoadingFastResume':
    case 'StartingTorrent':
    case 'LoadingMetadata':
      return [ deleteAction ];
    case 'DownloadingFiles':
      return [ pauseAction, deleteAction ];
    case 'DownloadPaused':
      return [ resumeAction, deleteAction ];
    case 'StoppingTorrent':
      return [ deleteAction ];
    case 'Completed':
      return [ archiveAction ];
    case 'Deleting':
      return [];
    case 'Failed':
      return [ deleteAction ];
    default:
      return [];
  }
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

function ComfirmDeleteDialog({ tracker, open, onClose, onConfirm }: {
  tracker: DownloadTracker,
  open: boolean,
  onClose: () => void,
  onConfirm: () => Promise<void>,
}): React.ReactElement {
  const [ loading, setLoading ] = useState(false);
  async function handleConfirm() {
    setLoading(true);
    try {
      await onConfirm();
      onClose();
    } catch (error) {
      console.error('Failed to delete download:', error);
    } finally {
      setLoading(false);
    }
  };
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Confirm Delete</DialogTitle>
      <DialogContent>
        Are you sure you want to delete the download for "{tracker.name}"?
        This action cannot be undone, and any downloaded files will be permanently removed.
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="primary">
          Cancel
        </Button>
        <Button onClick={handleConfirm} color="error" loading={loading}>
          Delete
        </Button>
      </DialogActions>
    </Dialog>
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
    case 'AddingTorrent':
    case 'MappingFiles':
    case 'LoadingFastResume':
    case 'StartingTorrent':
    case 'LoadingMetadata':
      return <Typography color="textSecondary"><Pending {...props} color="primary" />&nbsp;Initializing</Typography>;
    case 'DownloadingFiles':
      return <Typography color="textSecondary"><Downloading {...props} color="primary" />&nbsp;Downloading</Typography>;
    case 'DownloadPaused':
      return <Typography color="textSecondary"><PauseCircle {...props} color="warning" />&nbsp;Paused</Typography>;
    case 'StoppingTorrent':
      return <Typography color="textSecondary"><CheckCircle {...props} color="success" />&nbsp;Completing</Typography>;
    case 'Completed':
      return <Typography color="textSecondary"><CheckCircle {...props} color="success" />&nbsp;Completed</Typography>;
    case 'Deleting':
      return <Typography color="textSecondary"><Delete {...props} color="error" />&nbsp;Deleting</Typography>;
    case 'Failed':
      return <Typography color="textSecondary"><Error {...props} color="error" />&nbsp;Failed</Typography>;
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
    case 'AddingTorrent':
    case 'MappingFiles':
    case 'LoadingFastResume':
    case 'StartingTorrent':
    case 'LoadingMetadata':
      return { variant: 'query', color: 'primary' };
    case 'DownloadingFiles':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent };
    case 'DownloadPaused':
      return { variant: 'buffer', value: download.partialProgressPercent, valueBuffer: download.targetProgressPercent, color: 'warning' };
    case 'StoppingTorrent':
      return { variant: 'indeterminate', color: 'success' };
    case 'Completed':
      return { variant: 'buffer', value: 100, valueBuffer: 100, color: 'success' };
    case 'Deleting':
      return { variant: 'indeterminate', color: 'error' };
    case 'Failed':
      return { variant: 'query', color: 'error' };
    default:
      return { variant: 'indeterminate' };
  }
};
