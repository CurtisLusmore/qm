import {
  createBrowserRouter,
  Outlet,
  ScrollRestoration,
  RouterProvider,
} from 'react-router-dom';
import {
  colors,
  createTheme,
  Container,
  CssBaseline,
  ThemeProvider,
} from '@mui/material';
import {
  CollectionContextProvider,
  NavigationTabs,
} from './components';
import {
  Home,
  Movies,
  Playlist,
  Series,
} from './pages';
import { createCollectionContext } from './contexts';

const theme = createTheme({
  palette: {
    mode: 'dark',
    background: {
      default: colors.grey[900],
    },
  },
});

const router = createBrowserRouter([
  {
    path: '/',
    element: <RootLayout />,
    children: [
      {
        path: '',
        element: <Home />,
      },
      {
        path: 'movies',
        element: <Movies />,
      },
      {
        path: 'series',
        element: <Series />,
      },
      {
        path: 'playlist',
        element: <Playlist />,
      }
    ],
  }
], {
  basename: '/qm/',
});

function RootLayout(): React.ReactElement {
  const collection = createCollectionContext();
  return (
    <ThemeProvider theme={theme}>
      <CollectionContextProvider value={collection}>
        <CssBaseline />
        <NavigationTabs />
        <Container maxWidth="md" sx={{ position: 'relative', minHeight: '100vh' }}>
          { collection.loaded && <Outlet /> }
          <ScrollRestoration />
        </Container>
      </CollectionContextProvider>
    </ThemeProvider>
  );
};

export default function App(): React.ReactElement {
  return (
    <RouterProvider router={router} />
  );
};
