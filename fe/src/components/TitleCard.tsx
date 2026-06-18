import {
  Card,
  CardContent,
  CardMedia,
  CardHeader,
  Chip,
  Divider,
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
import type { CollectionStatus, Title } from '../types';

export default function TitleCard({ children, title, addToCollection, markWatched }: {
  children?: React.ReactNode,
  title: CollectionStatus<Title>,
  addToCollection: (title: CollectionStatus<Title>) => void,
  markWatched: (titleId: string) => void
}): React.ReactElement {
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
    title.inCollection && title.downloadStatus === 'not_downloaded' && <Chip key="download" icon={<Download />} label="Download" />,
    title.inCollection && title.downloadStatus === 'downloading' && <Chip key="downloading" icon={<Downloading />} label="Downloading" />,
    title.inCollection && title.downloadStatus === 'downloaded' && <Chip key="downloaded" icon={<DownloadDone />} label="Downloaded" />,
  ];

  return (
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
          src={title.trailerUrl}
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
