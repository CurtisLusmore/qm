import {
  useMemo,
  useState,
} from 'react';
import {
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

  return <>
    <Dialog
      open={open}
      fullWidth
      maxWidth='lg'
      onClose={() => setOpen(false)}
      disableRestoreFocus
    >
      <DialogTitle>Search</DialogTitle>
      <IconButton
        onClick={() => setOpen(false)}
        sx={{
          position: 'absolute',
          right: 8,
          top: 8,
        }}
      ><Close /></IconButton>
      <DialogContent>
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
        <Table stickyHeader>
          <TableHead>
            <TableRow>
            <TableCell width='100px'>Size</TableCell>
            <TableCell width='50px'>Seeders</TableCell>
            <TableCell>Name</TableCell> 
            </TableRow>
          </TableHead>
          <TableBody>
            {loading && <TableRow><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell><TableCell><Skeleton animation='pulse' /></TableCell></TableRow>}
            {!loading && terms.length === 0 && <TableRow><TableCell align='center' colSpan={5}>No results. Enter your search terms</TableCell></TableRow>}
            {!loading && terms.length > 0 && results.length === 0 && <TableRow><TableCell align='center' colSpan={5}>No results found. Try a different search</TableCell></TableRow>}
            {!loading && results.map(ListRow)}
          </TableBody>
        </Table>
      </DialogContent>
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
    const createClickHandler = function (infoHash: string): React.MouseEventHandler {
      return async function (): Promise<void> {
        setTerms('');
        setResults([]);
        setOpen(false);
        await saveTorrent(infoHash);
      };
    };

    return <TableRow
      key={result.infoHash}
      onClick={createClickHandler(result.infoHash)}
      hover
      style={{ cursor: 'pointer' }}
    >
      <TableCell>{Util.FormatBytes(result.sizeBytes)}</TableCell>
      <TableCell>{result.seeders}</TableCell>
      <TableCell>{result.name}</TableCell>
    </TableRow>;
  };
};