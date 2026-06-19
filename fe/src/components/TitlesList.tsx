import { useState } from 'react';
import {
  useMediaQuery,
  IconButton,
  ImageList,
  ImageListItem,
  ImageListItemBar,
  InputAdornment,
  Paper,
  TextField,
} from '@mui/material';
import {
  BookmarkRemove,
  Close,
  FilterList,
} from '@mui/icons-material';
import { useAutofocus } from '../hooks';
import type { Title } from '../types';

export default function TitlesList({ titles, navigate, remove }: {
  titles: Title[],
  navigate: (titleId: string) => void,
  remove: (titleId: string) => void,
}): React.ReactElement {
  const [ searchTerm, setSearchTerm ] = useState('');
  const sm = useMediaQuery((theme: any) => theme.breakpoints.down('sm'));
  const md = useMediaQuery((theme: any) => theme.breakpoints.down('md'));
  const ref = useAutofocus();

  function handleChange(event: React.ChangeEvent<HTMLInputElement>) {
    setSearchTerm(event.target.value);
  };
  
  function handleClickClear() {
    setSearchTerm('');
  };

  const filteredTitles = searchTerm.trim()
    ? titles.filter(title => title.name.toLowerCase().includes(searchTerm.trim().toLowerCase()))
    : titles;

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
          label="Filter Titles"
          placeholder="Filter titles in your collection"
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
              endAdornment: searchTerm ? (
                <InputAdornment position="end">
                  <Close onClick={handleClickClear} sx={{ cursor: 'pointer' }} />
                </InputAdornment>
              ) : (
                <InputAdornment position="end">
                  <FilterList />
                </InputAdornment>
              )
            }
          }}
        />
      </Paper>
      <ImageList cols={sm ? 1 : md ? 2 : 3} gap={8} rowHeight={425}>
        {filteredTitles.map((item: Title) => (
          <TitleItem
            key={item.id}
            item={item}
            remove={remove}
            navigate={navigate}
          />
        ))}
      </ImageList>
    </>
  );
};

function TitleItem({ item, remove, navigate }: {
  item: Title,
  remove: (titleId: string) => void,
  navigate: (titleId: string) => void,
}): React.ReactElement {
  const title = item.year ? `${item.name} (${item.year})` : item.name;
  const subtitle = item.type === 'movie' ? 'Movie' : item.type === 'series' ? 'TV Series' : 'Episode';

  function handleClickTitle(ev: React.MouseEvent) {
    ev.stopPropagation();
    navigate(item.id);
  };

  function handleClickRemove(ev: React.MouseEvent) {
    ev.stopPropagation();
    remove(item.id);
  };

  return (
    <ImageListItem
      key={item.id}
      sx={{ cursor: 'pointer' }}
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
        actionIcon={
          <IconButton
            title="Remove from Collection"
            onClick={handleClickRemove}
          >
            <BookmarkRemove />
          </IconButton>
        }
      />
    </ImageListItem>
   );
};
