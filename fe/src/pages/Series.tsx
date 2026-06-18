import { useSearchParams } from 'react-router-dom';
import { Series, SeriesList } from '../components';

export default function SeriesPage(): React.ReactElement {
  const [ searchParams ] = useSearchParams();
  const id = searchParams.get('id');

  return id
    ? <Series id={id} />
    : <SeriesList />;
};
