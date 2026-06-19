import { useNavigate } from 'react-router-dom';
import {
  Button,
  Paper,
  Typography,
} from '@mui/material';
import { TitlesList } from '../components';
import { useCollection } from '../hooks';

export default function SeriesList() {
  const collection = useCollection();
  const titles = collection.series;
  const navigate = useNavigate();

  return titles.length === 0 ? (
    <Paper
      sx={{
        padding: 2,
        position: 'absolute',
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
      }}
    >
      <Typography variant="h6" align="center">
        Your series collection is empty!
      </Typography>
      <Typography variant="body1" align="center">
        Search for series from the Home page and add them to your collection,
        then they will appear here.
      </Typography>
      <Button
        variant="contained"
        color="primary"
        onClick={() => navigate('/')}
        sx={{ display: 'block', mx: 'auto', mt: 2 }}
      >
        Search for series
      </Button>
    </Paper>
  ) : (
    <TitlesList
      titles={titles}
      navigate={(titleId: string) => navigate(`/series?id=${titleId}`)}
      remove={collection.remove}
    />
  );
};
