import { useEffect, useState } from 'react';
import {
  Button,
  Card,
  CardActions,
  CardHeader,
  Dialog,
  DialogTitle,
  DialogContent,
  Skeleton,
  TextField,
  Typography,
  IconButton,
} from '@mui/material';
import {
  Close,
} from '@mui/icons-material';
import { searchDownloads, startDownload } from '../clients';
import { useDebounced, useDispatchToast } from '../hooks';
import type { DownloadSearchResult, Title } from '../types';

export default function DownloadSearch({ title, open, onClose }: {
  title: Title,
  open: boolean,
  onClose: () => void
}): React.ReactElement {
  const [ searchTerm, setSearchTerm ] = useState(`${title.name} (${title.year})`);
  const [ searchResults, setSearchResults ] = useState([] as DownloadSearchResult[]);
  const [ loaded, setLoaded ] = useState(false);
  const dispatchToast = useDispatchToast();
  const debouncedSearchDownloads = useDebounced(searchDownloads, 200);

  useEffect(() => {
    if (!open) return;

    (async function () {
      try {
        setLoaded(false);
        const results = await debouncedSearchDownloads(searchTerm);
        setSearchResults(results);
        setLoaded(true);
      } catch (error) {
        console.error(error);
        dispatchToast(`Failed to fetch search results: ${error}`, 'error');
      }
    }());
  }, [ title, open, searchTerm ]);

  async function handleClickDownload(infoHash: string) {
    try {
      await startDownload(infoHash, title);
      dispatchToast('Download started successfully', 'success');
      handleClose();
    } catch (error) {
      console.error(error);
      dispatchToast(`Failed to start download: ${error}`, 'error');
    }
  }

  function handleClose() {
    setSearchTerm(`${title.name} (${title.year})`);
    setSearchResults([]);
    setLoaded(false);
    onClose();
  }

  return (
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
      <DialogTitle>Download Search
        <IconButton onClick={handleClose} color="inherit" sx={{ position: 'absolute', right: 8, top: 8 }}>
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent>
        <TextField
          fullWidth
          variant="outlined"
          placeholder={`${title.name} (${title.year})`}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          sx={{ mb: 2 }}
        />
        {!loaded && (
          [1, 2, 3].map((_, index) => (
            <Card key={index} variant="outlined" sx={{ mb: 2 }}>
              <CardHeader
                title={<Skeleton variant="text" width={200} />}
                subheader={<Skeleton variant="text" width={150} />}
              />
              <CardActions>
                <Skeleton variant="rectangular" width={100} />
              </CardActions>
            </Card>
          ))
        )}
        {loaded && searchResults.length === 0 && <Typography>No results found.</Typography>}
        {loaded && searchResults.length > 0 && (
          searchResults.map(result => (
            <Card key={result.infoHash} variant="outlined" sx={{ mb: 2 }}>
              <CardHeader
                title={result.name}
                subheader={`${formatBytes(result.sizeBytes)} \u00A0\u2022\u00A0 ${result.seeders} seeders`}
              />
              <CardActions>
                <Button onClick={() => handleClickDownload(result.infoHash)} color="primary">Download</Button>
              </CardActions>
            </Card>
          ))
        )}
      </DialogContent>
    </Dialog>
  );
};

function formatBytes(bytes: number): string {
  if (bytes === 0) return '0B';
  const k = 1000;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + sizes[i];
};
