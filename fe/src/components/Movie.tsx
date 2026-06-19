import { useEffect, useState } from 'react';
import { getTitle } from '../clients';
import { TitleCard } from '../components';
import { useCollection } from '../hooks';
import type { CollectionStatus, Movie } from '../types';

export default function Movie({ id }: { id: string }) {
  const collection = useCollection();
  const [ title, setTitle ] = useState<CollectionStatus<Movie> | undefined>(undefined);

  useEffect(() => {
    (async function () {
      let title = collection.get(id!) as CollectionStatus<Movie> | undefined;
      if (title) {
        setTitle(title);
      }

      title = collection.check(await getTitle(id!)) as CollectionStatus<Movie>;
      setTitle(title);
    }());
  }, [ collection, id ]);

  return (
    <TitleCard title={title} markWatched={collection.markWatched} addToCollection={collection.add} />
  );
};
