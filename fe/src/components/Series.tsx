import { useEffect, useState } from 'react';
import {
  CardContent,
  CardHeader,
  CardMedia,
  Divider,
  Stack,
} from '@mui/material';
import { getTitle } from '../clients';
import { TitleCard } from '../components';
import { useCollection } from '../hooks';
import type { CollectionStatus, Episode, Series } from '../types';

export default function Series({ id }: { id: string }) {
  const collection = useCollection();
  const [ title, setTitle ] = useState<CollectionStatus<Series> | undefined>(undefined);

  useEffect(() => {
    (async function () {
      const fetchedTitle = collection.get(id!) || collection.check(await getTitle(id!));
      setTitle(fetchedTitle as CollectionStatus<Series>);
    }());
  }, [ collection, id ]);

  return title === undefined ? null : (
    <TitleCard title={title} markWatched={collection.markWatched} addToCollection={collection.add}>
      <Stack
        divider={<Divider />}
      >
        {title.episodes?.map(episode => (
          <EpisodeSection key={episode.id} episode={episode} />
        ))}
      </Stack>
    </TitleCard>
  );
};

function EpisodeSection({ episode }: { episode: Episode }) {
  const subheaders = [
    `S${leftPad(episode.seasonNumber, 2)}E${leftPad(episode.episodeNumber, 2)}`,
    episode.runtimeSeconds !== undefined ? `${Math.floor(episode.runtimeSeconds / 60)} min` : '',
    `${episode.ratings.rating}/10 (${formatLargeNumber(episode.ratings.count)} votes)`,
  ].filter(Boolean).join('\u00A0\u2022\u00A0');
  return (
    <Stack direction="row" spacing={1} sx={{ padding: 1 }}>
      <CardMedia
        component="img"
        image={episode.imageUrl}
        alt={episode.name}
        sx={{ width: 150, height: 100, objectFit: 'cover', flexShrink: 0 }}
      />
      <CardContent sx={{ py: 0 }}>
        <CardHeader
          title={episode.name}
          subheader={<>{subheaders}</>}
          sx={{ padding: 0 }}
        />
        {episode.plot}
      </CardContent>
    </Stack>
  )
};

function leftPad(num: number, size: number) {
  let s = num.toString();
  while (s.length < size) {
    s = '0' + s;
  }
  return s;
};

function formatLargeNumber(num: number): string {
  if (num >= 1_000_000_000) {
    return (num / 1_000_000_000).toFixed(1) + 'B';
  } else if (num >= 1_000_000) {
    return (num / 1_000_000).toFixed(1) + 'M';
  } else if (num >= 1_000) {
    return (num / 1_000).toFixed(1) + 'K';
  } else {
    return num.toString();
  }
};