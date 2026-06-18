import { useNavigate } from 'react-router-dom';
import { TitlesList } from '../components';
import { useCollection } from '../hooks';

export default function SeriesList() {
  const collection = useCollection();
  const titles = collection.series;
  const navigate = useNavigate();

  return (
    <TitlesList
      titles={titles}
      navigate={(titleId: string) => navigate(`/series?id=${titleId}`)}
      remove={collection.remove}
    />
  );
};
