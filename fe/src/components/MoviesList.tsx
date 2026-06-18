import { useNavigate } from 'react-router-dom';
import { TitlesList } from '../components';
import { useCollection } from '../hooks';

export default function MoviesList() {
  const collection = useCollection();
  const titles = collection.movies;
  const navigate = useNavigate();

  return (
    <TitlesList
      titles={titles}
      navigate={(titleId: string) => navigate(`/movies?id=${titleId}`)}
      remove={collection.remove}
    />
  );
};
