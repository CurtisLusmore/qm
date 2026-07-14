import { useState } from 'react';
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
  DownloadsContextProvider,
  DownloadTracker,
  NavigationTabs,
  ServerEventsContextProvider,
  ThemeSwitcher,
  ToastsContextProvider,
} from './components';
import {
  Home,
  Movies,
  Playlist,
  Series,
} from './pages';
import {
  createCollectionContext,
  createDownloadsContext,
  createServerEventsContext,
} from './contexts';

const lightTheme = createTheme({
  palette: {
    mode: 'light',
    background: {
      default: colors.grey[100],
    },
  },
});

const darkTheme = createTheme({
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
]);

function RootLayout(): React.ReactElement {
  const [ theme, setTheme ] = useState<'light' | 'dark'>('dark');
  const registration = createServerEventsContext();
  const collection = createCollectionContext();
  const downloads = createDownloadsContext(registration);
  return (
    <ThemeProvider theme={theme === 'light' ? lightTheme : darkTheme}>
      <ToastsContextProvider>
        <ServerEventsContextProvider value={registration}>
          <DownloadsContextProvider value={downloads}>
            <CollectionContextProvider value={collection}>
              <CssBaseline />
              <NavigationTabs />
              <ThemeSwitcher theme={theme} setTheme={setTheme} />
              <Container maxWidth="md" sx={{ position: 'relative', minHeight: '100vh' }}>
                { collection.loaded && <Outlet /> }
                <ScrollRestoration />
              </Container>
              <DownloadTracker />
            </CollectionContextProvider>
          </DownloadsContextProvider>
        </ServerEventsContextProvider>
      </ToastsContextProvider>
    </ThemeProvider>
  );
};

export default function App(): React.ReactElement {
  return (
    <RouterProvider router={router} />
  );
};
