import { useSearchParams } from 'react-router-dom';
import { Movie, MoviesList } from '../components';

export default function MoviesPage(): React.ReactElement {
  const [ searchParams ] = useSearchParams();
  const id = searchParams.get('id');

  return id
    ? <Movie id={id} />
    : <MoviesList />;
};
