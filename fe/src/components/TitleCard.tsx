import { useEffect, useState } from 'react';
import {
  Card,
  CardContent,
  CardMedia,
  CardHeader,
  Chip,
  Divider,
  Skeleton,
  Stack,
} from '@mui/material';
import {
  BookmarkAdd,
  Download,
  DownloadDone,
  Downloading,
  Visibility,
  VisibilityOutlined,
} from '@mui/icons-material';
import { DownloadSearch } from '.';
import type { CollectionStatus, Title } from '../types';

export default function TitleCard({ children, title, addToCollection, markWatched }: {
  children?: React.ReactNode,
  title: CollectionStatus<Title> | undefined,
  addToCollection: (title: CollectionStatus<Title>) => void,
  markWatched: (titleId: string) => void
}): React.ReactElement {
  return title === undefined ? (
    <TitleCardSkeleton>{children}</TitleCardSkeleton>
  ) : (
    <TitleCardInner title={title} addToCollection={addToCollection} markWatched={markWatched}>
      {children}
    </TitleCardInner>
  );
};

function TitleCardInner({ children, title, addToCollection, markWatched }: {
  children?: React.ReactNode,
  title: CollectionStatus<Title>,
  addToCollection: (title: CollectionStatus<Title>) => void,
  markWatched: (titleId: string) => void
}): React.ReactElement {
  const [ downloadSearchOpen, setDownloadSearchOpen ] = useState(false);
  const lastWatched = title.watched && title.lastWatched ? getRelativeDateText(title.lastWatched) : null;
  const subheaders = [
    title.year,
    title.runtimeSeconds !== undefined ? `${Math.floor(title.runtimeSeconds / 60)} min` : null,
    title.classification,
    `${title.ratings.rating}/10 (${formatLargeNumber(title.ratings.count)} votes)`,
  ].filter(Boolean).join('\u00A0\u00A0\u2022\u00A0\u00A0');

  const chips = [
    !title.inCollection && <Chip key="add" icon={<BookmarkAdd />} label="Add to Collection" onClick={() => addToCollection(title)} />,
    title.inCollection && title.watched && title.lastWatched && <Chip key="watched" icon={<Visibility />} label={lastWatched} title={`Last watched ${lastWatched}`} />,
    title.inCollection && (!title.watched || !title.lastWatched) && <Chip key="mark" icon={<VisibilityOutlined />} label="Mark Watched" onClick={() => markWatched(title.id)} />,
    title.inCollection && title.downloadStatus === 'not_downloaded' && <Chip key="download" icon={<Download />} label="Download" onClick={() => setDownloadSearchOpen(true)} />,
    title.inCollection && title.downloadStatus === 'downloading' && <Chip key="downloading" icon={<Downloading />} label="Downloading" />,
    title.inCollection && title.downloadStatus === 'downloaded' && <Chip key="downloaded" icon={<DownloadDone />} label="Downloaded" />,
  ];

  const [ videoUrl, setVideoUrl ] = useState<string | undefined>(title.trailerUrl);

  useEffect(() => {
    (async function () {
      const resp = await fetch(`http://localhost:5138/movies/${title.id}/media`, { method: 'HEAD' });
      if (resp.ok) {
        setVideoUrl(`http://localhost:5138/movies/${title.id}/media`);
      }
    }());
  }, []);

  return (
    <>
      <Card sx={{ elevation: 2, marginTop: 2 }}>
        <CardHeader
          title={title.name}
          subheader={<>{subheaders}</>}
        />
        <CardContent>
          <Stack direction="row" spacing={1}>
            {chips}
          </Stack>
        </CardContent>
        <Stack direction="row" spacing={1} sx={{ padding: 1 }}>
          <CardMedia
            component="img"
            image={title.imageUrl}
            alt={title.name}
            sx={{ width: 200, height: 300, objectFit: 'cover' }}
          />
          <CardMedia
            component="video"
            src={videoUrl}
            controls
            sx={{ height: 300, objectFit: 'cover' }}
          />
        </Stack>
        <CardContent>
          {title.plot}
        </CardContent>
        <Divider />
        <CardContent>
          <strong>Directors</strong>&nbsp;&nbsp;&nbsp;{title.directors?.map(d => d.name).join(', ')}
        </CardContent>
        <Divider />
        <CardContent>
          <strong>Writers</strong>&nbsp;&nbsp;&nbsp;{title.writers?.map(w => w.name).join(', ')}
        </CardContent>
        <Divider />
        <CardContent>
          <strong>Stars</strong>&nbsp;&nbsp;&nbsp;{title.cast?.map(c => c.name).join(', ')}
        </CardContent>
        {children && <>
          <Divider />
          {children}
        </>}
      </Card>
      <DownloadSearch
        title={title}
        open={downloadSearchOpen}
        onClose={() => setDownloadSearchOpen(false)}
      />
    </>
  );
};

function TitleCardSkeleton({ children }: { children?: React.ReactNode }): React.ReactElement {
  return (
    <Card sx={{ elevation: 2, marginTop: 2 }}>
      <CardHeader
        title={<Skeleton variant="text" width={200} />}
        subheader={<Skeleton variant="text" width={150} />}
      />
      <CardContent>
        <Stack direction="row" spacing={1}>
          <Skeleton variant="rectangular" width={100} height={40} />
        </Stack>
      </CardContent>
      <Stack direction="row" spacing={1} sx={{ padding: 1 }}>
        <Skeleton variant="rectangular" width={200} height={300} />
        <Skeleton variant="rectangular" width={'100%'} height={300} sx={{ flexGrow: 1 }} />
      </Stack>
      <CardContent>
        <Skeleton variant="text" width={'100%'} />
      </CardContent>
      <Divider />
      <CardContent>
        <Skeleton variant="text" width={100} sx={{ display: 'inline-block' }} />&nbsp;&nbsp;&nbsp;<Skeleton variant="text" width={200} sx={{ display: 'inline-block' }} />
      </CardContent>
      <Divider />
      <CardContent>
        <Skeleton variant="text" width={100} sx={{ display: 'inline-block' }} />&nbsp;&nbsp;&nbsp;<Skeleton variant="text" width={200} sx={{ display: 'inline-block' }} />
      </CardContent>
      <Divider />
      <CardContent>
        <Skeleton variant="text" width={100} sx={{ display: 'inline-block' }} />&nbsp;&nbsp;&nbsp;<Skeleton variant="text" width={200} sx={{ display: 'inline-block' }} />
      </CardContent>
      {children && <>
        <Divider />
        {children}
      </>}
    </Card>
  );
};

function getRelativeDateText(date: Date): string {
  const now = new Date();
  const diffDays = getDate(now) - getDate(date);
  if (diffDays === 0) {
    return 'Today';
  } else if (diffDays === 1) {
    return 'Yesterday';
  } else if (diffDays < 7) {
    return `${diffDays}d ago`;
  } else if (diffDays < 30) {
    const diffWeeks = Math.floor(diffDays / 7);
    return `${diffWeeks}w ago`;
  } else if (diffDays < 365) {
    const diffMonths = Math.floor(diffDays / 30);
    return `${diffMonths}mo ago`;
  } else {
    const diffYears = Math.floor(diffDays / 365);
    return `${diffYears}y ago`;
  }

  function getDate(date: Date): number {
    return Math.floor(date.getTime() / (1000 * 60 * 60 * 24));
  };
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
