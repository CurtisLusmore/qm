import { useEffect, useState } from 'react';
import {
  Button,
  Card,
  CardActions,
  CardHeader,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Skeleton,
  Typography,
} from '@mui/material';
import {
  Close,
} from '@mui/icons-material';
import { searchDownloads, startDownload } from '../clients';
import { useDispatchToast } from '../hooks';
import type { DownloadSearchResult, Title } from '../types';

export default function DownloadSearch({ title, open, onClose }: {
  title: Title,
  open: boolean,
  onClose: () => void
}): React.ReactElement {
  const [ searchResults, setSearchResults ] = useState([] as DownloadSearchResult[]);
  const [ loaded, setLoaded ] = useState(false);
  const dispatchToast = useDispatchToast();

  useEffect(() => {
    if (!open) return;

    (async function () {
      try {
        const results = await searchDownloads(title);
        setSearchResults(results);
        setLoaded(true);
      } catch (error) {
        console.error(error);
        dispatchToast(`Failed to fetch search results: ${error}`, 'error');
      }
    }());
  }, [ title, open ]);

  async function handleClickDownload(infoHash: string) {
    try {
      await startDownload(infoHash, title);
      dispatchToast('Download started successfully', 'success');
      onClose();
    } catch (error) {
      console.error(error);
      dispatchToast(`Failed to start download: ${error}`, 'error');
    }
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="md">
      <DialogTitle>Search for "{title.name}"
        <Button onClick={onClose} color="inherit" startIcon={<Close />} sx={{ position: 'absolute', right: 8, top: 8 }} />
      </DialogTitle>
      <DialogContent>
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
                subheader={`${formatSize(result.sizeBytes)} \u00A0\u2022\u00A0 ${result.seeders} seeders`}
              />
              <CardActions>
                <Button onClick={() => handleClickDownload(result.infoHash)} color="primary">Download</Button>
              </CardActions>
            </Card>
          ))
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="secondary">Close</Button>
      </DialogActions>
    </Dialog>
  );
};

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(2)} KB`;
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
};
