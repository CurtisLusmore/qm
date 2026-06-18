import {
  useLocation,
  useNavigate,
} from 'react-router-dom';
import {
  useMediaQuery,
  Tab,
  Tabs,
  type SxProps,
} from '@mui/material';
import {
  Home,
  Movie,
  PlaylistPlay,
  Tv,
} from '@mui/icons-material';

export default function NavigationTabs(): React.ReactElement {
  const location = useLocation();
  const navigate = useNavigate();
  const selectedTab = location.pathname;
  const mobile = useMediaQuery((theme: any) => theme.breakpoints.down('sm'));

  const style = mobile
    ? {
      position: 'fixed',
      bottom: 0,
      left: '50%',
      transform: 'translateX(-50%)',
      width: '100%',
      justifyContent: 'space-between',
      zIndex: (theme: any) => theme.zIndex.appBar,
    }
    : {
      position: 'fixed',
      top: '50%',
      left: 0,
      transform: 'translateY(-50%)',
    } as SxProps;

  return (
    <Tabs
      orientation={mobile ? 'horizontal' : 'vertical'}
      value={selectedTab}
      variant="fullWidth"
      sx={{ ...style }}
    >
      <Tab icon={<Home />} title="Home" value="/" onClick={() => navigate('/')} />
      <Tab icon={<Movie />} title="Movies" value="/movies" onClick={() => navigate('/movies')} />
      <Tab icon={<Tv />} title="TV Series" value="/series" onClick={() => navigate('/series')} />
      <Tab icon={<PlaylistPlay />} title="Playlist" value="/playlist" onClick={() => navigate('/playlist')} />
    </Tabs>
  )
};
