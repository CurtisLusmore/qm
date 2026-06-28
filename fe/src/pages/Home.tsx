import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  useMediaQuery,
  IconButton,
  ImageList,
  ImageListItem,
  ImageListItemBar,
  InputAdornment,
  Paper,
  Skeleton,
  TextField,
  Container,
  Typography,
} from '@mui/material';
import {
  Close,
  BookmarkAdd,
  BookmarkRemove,
  Search as SearchIcon,
} from '@mui/icons-material';
import { getSuggestions, getTitle } from '../clients';
import { useAutofocus, useCaching, useCollection, useDebounced } from '../hooks';
import type { CollectionStatus, Title, TitleSummary } from '../types';

export default function Search() {
  const navigate = useNavigate();
  const collection = useCollection();
  const recentlyAdded = collection.recentlyAdded;

  const [ loading, setLoading ] = useState(false);
  const [ searchTerm, setSearchTerm ] = useState('');
  const [ searchResults, setSearchResults ] = useState<TitleSummary[]>([]);

  const results: CollectionStatus<TitleSummary>[] = useMemo(
    () => collection.check(searchResults),
    [ collection, searchResults ]
  );

  const hasResults = !loading && searchTerm;
  const noResults = !loading && searchTerm && results.length === 0;

  const debouncedGetSuggestions = useDebounced(getSuggestions, 200);
  const cachedGetTitle = useCaching(getTitle);

  useEffect(() => {
    if (searchTerm) setLoading(true);
    debouncedGetSuggestions(searchTerm)
      .then(results => {
        setSearchResults(results);
        setLoading(false);
      }).catch(() => {});
  }, [ searchTerm ]);

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    setSearchTerm(event.target.value);
  };

  function handleClickClear() {
    setSearchTerm('');
  };

  const sm = useMediaQuery((theme: any) => theme.breakpoints.down('sm'));
  const md = useMediaQuery((theme: any) => theme.breakpoints.down('md'));
  const ref = useAutofocus();

  return (
    <>
      <Paper
        elevation={2}
        sx={{
          p: 2,
          position: 'sticky',
          top: 0,
          zIndex: 1,
        }}
      >
        <TextField
          label="Search Titles"
          placeholder="Search for movies or TV series to add to your collection"
          variant="outlined"
          value={searchTerm}
          onChange={handleChange}
          inputRef={ref}
          fullWidth
          slotProps={{
            htmlInput: {
              enterKeyHint: 'search',
            },
            input: {
              endAdornment: hasResults ? (
                <InputAdornment position="end">
                  <Close onClick={handleClickClear} sx={{ cursor: 'pointer' }} />
                </InputAdornment>
              ) : (
                <InputAdornment position="end">
                  <SearchIcon />
                </InputAdornment>
              )
            },
          }}
        />
      </Paper>
      {
        !searchTerm && (
          <Container sx={{ py: 4 }}>
            <Typography>Recently Added</Typography>
            <ImageList
              cols={3} gap={8}
              rowHeight={400}
              sx={{ mt: 0, overflowX: 'scroll', display: 'flex', flexWrap: 'nowrap' }}
            >
              {recentlyAdded.map((item: Title) => (
                <TitleItem key={item.id} item={item} navigate={navigate} />
              ))}
            </ImageList>
          </Container>
        )
      }
      {loading && (
        <ImageList cols={sm ? 1 : md ? 2 : 3} gap={8} rowHeight={400}>
          <ImageListItem key='skeleton-1'>
            <Skeleton variant="rectangular" height="100%" />
          </ImageListItem>
          <ImageListItem key='skeleton-2'>
            <Skeleton variant="rectangular" height="100%" />
          </ImageListItem>
          <ImageListItem key='skeleton-3'>
            <Skeleton variant="rectangular" height="100%" />
          </ImageListItem>
        </ImageList>
      )}
      {hasResults &&(
        <ImageList cols={sm ? 1 : md ? 2 : 3} gap={8} rowHeight={400}>
          {results?.map((item: CollectionStatus<TitleSummary>) => (
            <SearchResultItem
              key={item.id}
              item={item}
              add={collection.add}
              remove={collection.remove}
              getTitle={cachedGetTitle}
              navigate={navigate}
            />
          ))}
        </ImageList>
      )}
      {noResults && (
        <Paper
          elevation={2}
          sx={{ p: 2, my: 2 }}
        >
          No results found
        </Paper>
      )}
    </>
  );
};

function SearchResultItem({ item, add, remove, getTitle, navigate }: {
  item: CollectionStatus<TitleSummary>,
  add: (title: Title) => void,
  remove: (titleId: string) => void,
  getTitle: (titleId: string) => Promise<Title>,
  navigate: (path: string) => void,
}) {
  const [ loading, setLoading ] = useState(false);
  const title = item.year ? `${item.name} (${item.year})` : item.name;
  const subtitle = item.type === 'movie' ? 'Movie' : item.type === 'series' ? 'TV Series' : 'Episode';

  function handleMouseEnter() {
    getTitle(item.id).catch(() => {});
  };

  function handleClickTitle(ev: React.MouseEvent) {
    ev.stopPropagation();
    switch (item.type) {
      case 'movie':
        navigate(`/movies?id=${item.id}`);
        break;
      case 'series':
        navigate(`/series?id=${item.id}`);
        break;
    }
  };

  async function handleClickAdd(ev: React.MouseEvent) {
    ev.stopPropagation();
    setLoading(true);
    try {
      const title = await getTitle(item.id);
      add(title);
    } catch (error) {
      alert(error);
    }
    setLoading(false);
  };

  async function handleClickRemove(ev: React.MouseEvent) {
    ev.stopPropagation();
    setLoading(true);
    try {
      remove(item.id);
    } catch (error) {
      alert(error);
    }
    setLoading(false);
  };

  return (
    <ImageListItem
      key={item.id}
      sx={{ cursor: 'pointer' }}
      onMouseEnter={handleMouseEnter}
      onClick={handleClickTitle}
    >
      <img
        src={item.imageUrl}
        alt={item.name}
        style={{ width: '100%', height: '100%', objectFit: 'cover' }}
      />
      <ImageListItemBar
        title={title}
        subtitle={subtitle}
        actionIcon={item.inCollection ? (
          <IconButton
            title="Remove from Collection"
            onClick={handleClickRemove}
            loading={loading}
            sx={{ color: 'rgba(255, 255, 255)' }}
          >
            <BookmarkRemove />
          </IconButton>
        ) : (
          <IconButton
            title="Add to Collection"
            onClick={handleClickAdd}
            loading={loading}
            sx={{ color: 'rgba(255, 255, 255)' }}
          >
            <BookmarkAdd />
          </IconButton>
        )}
      />
    </ImageListItem>
   );
};

function TitleItem({ item, navigate }: { item: Title, navigate: (path: string) => void }): React.ReactElement {
  const title = item.year ? `${item.name} (${item.year})` : item.name;
  const subtitle = item.type === 'movie' ? 'Movie' : item.type === 'series' ? 'TV Series' : 'Episode';

  function handleClickTitle(ev: React.MouseEvent) {
    ev.stopPropagation();
    switch (item.type) {
      case 'movie':
        navigate(`/movies?id=${item.id}`);
        break;
      case 'series':
        navigate(`/series?id=${item.id}`);
        break;
    }
  };

  return (
    <ImageListItem
      key={item.id}
      sx={{ cursor: 'pointer', flexShrink: 0 }}
      onClick={handleClickTitle}
    >
      <img
        src={item.imageUrl}
        alt={item.name}
        style={{ width: '100%', height: '100%', objectFit: 'cover' }}
      />
      <ImageListItemBar
        title={title}
        subtitle={subtitle}
      />
    </ImageListItem>
   );
};
