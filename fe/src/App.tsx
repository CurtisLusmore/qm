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
  DownloadTracker,
  ToastsContextProvider,
  NavigationTabs,
  ThemeSwitcher,
} from './components';
import {
  Home,
  Movies,
  Playlist,
  Series,
} from './pages';
import { createCollectionContext } from './contexts';

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
], {
  basename: '/qm/',
});

function RootLayout(): React.ReactElement {
  const [ theme, setTheme ] = useState<'light' | 'dark'>('dark');
  const collection = createCollectionContext();
  return (
    <ThemeProvider theme={theme === 'light' ? lightTheme : darkTheme}>
      <CollectionContextProvider value={collection}>
        <ToastsContextProvider>
          <CssBaseline />
          <NavigationTabs />
          <ThemeSwitcher theme={theme} setTheme={setTheme} />
          <Container maxWidth="md" sx={{ position: 'relative', minHeight: '100vh' }}>
            { collection.loaded && <Outlet /> }
            <ScrollRestoration />
          </Container>
          <DownloadTracker />
        </ToastsContextProvider>
      </CollectionContextProvider>
    </ThemeProvider>
  );
};

export default function App(): React.ReactElement {
  return (
    <RouterProvider router={router} />
  );
};
