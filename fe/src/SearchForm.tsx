import {
  useEffect,
  useMemo,
  useState,
} from 'react';
import {
  useMediaQuery,
  useTheme,
  Box,
  Dialog,
  DialogContent,
  DialogTitle,
  Fab,
  FormControl,
  IconButton,
  Input,
  InputAdornment,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
} from '@mui/material';
import {
  Add,
  Close,
  Search,
} from '@mui/icons-material';
import {
  saveTorrent,
  searchTorrents,
  type TorrentSearchResult,
} from './Client';
import Util from './Util';

export default function SearchForm(): React.ReactElement {
  const [ open, setOpen ] = useState(false);
  const [ terms, setTerms ] = useState('');
  const [ loading, setLoading ] = useState(false);
  const [ results, setResults ] = useState([] as TorrentSearchResult[]);

  useEffect(() => {
    function handler(): void {
      setOpen(true);
    };
    window.addEventListener('search', handler);
    return () => window.removeEventListener('search', handler);
  }, []);

  const handleSearchInput = useMemo(function () {
    let handle: number | undefined = undefined;
    return async function handleSearchInput(event: React.FormEvent<HTMLInputElement>): Promise<void> {
      const terms = (event.target as HTMLInputElement).value
      setTerms(terms);

      clearTimeout(handle);
      handle = setTimeout(async function () {
        setLoading(true);
        try {
          const results = await searchTorrents(terms);
          setResults(results || []);
        }
        finally {
          setLoading(false);
        }
      }, 200);
    };
  }, []);

  const theme = useTheme();
  const isSmall = useMediaQuery(theme.breakpoints.down('sm'));

  return <>
    <Dialog
      open={open}
      fullWidth
      fullScreen={isSmall}
      maxWidth='lg'
      onClose={() => setOpen(false)}
      disableRestoreFocus
    >
      <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh', maxHeight: { xs: '100vh', sm: 'calc(100vh - 64px)' } }}>
        <DialogTitle>
          Search
          <IconButton
            onClick={() => setOpen(false)}
            sx={{
              position: 'absolute',
              right: 8,
              top: 8,
            }}
          ><Close /></IconButton>
        </DialogTitle>
        <DialogContent sx={{ flexShrink: 0, flexGrow: 0, paddingBlockEnd: 0 }}>
          <FormControl fullWidth>
            <Input
              value={terms}
              placeholder='Search for a torrent...'
              onInput={handleSearchInput}
              endAdornment={<InputAdornment position='end'><Search /></InputAdornment>}
              autoFocus
              fullWidth
            />
          </FormControl>
        </DialogContent>
        <DialogContent sx={{ flex: '1', overflow: 'hidden', paddingBlockStart: 0 }}>
          <TableContainer sx={{ height: '100%', overflowY: 'scroll' }}>
            <Table stickyHeader sx={{ tableLayout: 'fixed' }}>
              <TableHead>
                <TableRow>
                <TableCell width='50px'>Size</TableCell>
                <TableCell width='50px'>Seeders</TableCell>
                <TableCell width='100%'>Name</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {loading && <TableRow><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell></TableRow>}
                {!loading && terms.length === 0 && <TableRow><TableCell align='center' colSpan={5}>No results. Enter your search terms</TableCell></TableRow>}
                {!loading && terms.length > 0 && results.length === 0 && <TableRow><TableCell align='center' colSpan={5}>No results found. Try a different search</TableCell></TableRow>}
                {!loading && results.map(ListRow)}
              </TableBody>
            </Table>
          </TableContainer>
        </DialogContent>
      </Box>
    </Dialog>
    <Tooltip title='Search for and save a torrent' placement='left'>
      <Fab
        color='primary'
        sx={{ position: 'fixed', bottom: 16, right: 16 }}
        onClick={() => setOpen(true)}
      >
        <Add />
      </Fab>
    </Tooltip>
  </>;

  function ListRow(result: TorrentSearchResult): React.ReactElement {
    const createClickHandler = function (infoHash: string, name: string): React.MouseEventHandler {
      return async function (): Promise<void> {
        setTerms('');
        setResults([]);
        setOpen(false);
        await saveTorrent(infoHash, name);
      };
    };

    return <TableRow
      key={result.infoHash}
      onClick={createClickHandler(result.infoHash, result.name)}
      hover
      style={{ cursor: 'pointer' }}
    >
      <TableCell align='right'>{Util.FormatBytes(result.sizeBytes)}</TableCell>
      <TableCell align='right'>{result.seeders}</TableCell>
      <TableCell sx={{ overflowWrap: 'break-word' }}>{result.name}</TableCell>
    </TableRow>;
  };
};