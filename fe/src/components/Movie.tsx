import { useEffect, useState } from 'react';
import { getTitle } from '../clients';
import { TitleCard } from '../components';
import { useCollection } from '../hooks';
import type { Movie } from '../types';

export default function Movie({ id }: { id: string }) {
  const collection = useCollection();
  const [ title, setTitle ] = useState<Movie | undefined>(undefined);

  useEffect(() => {
    (async function () {
      let title = collection.get(id!) as Movie | undefined;
      if (title) {
        setTitle(title);
      }

      title = collection.check(await getTitle(id!)) as Movie;
      setTitle(title);
    }());
  }, [ collection.movies, id ]);

  return (
    <TitleCard title={title} markWatched={collection.markWatched} addToCollection={collection.add} />
  );
};
