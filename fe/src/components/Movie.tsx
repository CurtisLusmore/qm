import { useEffect, useState } from 'react';
import { getTitle } from '../clients';
import { TitleCard } from '../components';
import { useCollection } from '../hooks';
import type { CollectionStatus, Title } from '../types';

export default function Movie({ id }: { id: string }) {
  const collection = useCollection();
  const [ title, setTitle ] = useState<CollectionStatus<Title> | undefined>(undefined);

  useEffect(() => {
    (async function () {
      const fetchedTitle = collection.get(id!) || collection.check(await getTitle(id!));
      setTitle(fetchedTitle);
    }());
  }, [ collection, id ]);

  return (
    <TitleCard title={title} markWatched={collection.markWatched} addToCollection={collection.add} />
  );
};
