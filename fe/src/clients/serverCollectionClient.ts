import type { Movie, Series, ServerCollection } from '../types';

export async function getServerCollection(): Promise<ServerCollection> {
  const response = await fetch(`/api/collection`);
  if (!response.ok) {
    throw new Error(`Collection request failed with status ${response.status}`);
  }
  const collection = await response.json() as ServerCollection;
  return collection;
};

export async function addMovieToServerCollection(title: Movie): Promise<void> {
  await fetch('/api/movies', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(title),
  });
};

export async function addSeriesToServerCollection(title: Series): Promise<void> {
  await fetch('/api/series', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(title),
  });
};

export async function removeMovieFromServerCollection(titleId: string): Promise<void> {
  await fetch(`/api/movies/${titleId}`, {
    method: 'DELETE',
  });
};

export async function removeSeriesFromServerCollection(titleId: string): Promise<void> {
  await fetch(`/api/series/${titleId}`, {
    method: 'DELETE',
  });
};
