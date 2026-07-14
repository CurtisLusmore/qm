import type { Movie, Series } from '../types';

export async function getServerMovies(since?: Date): Promise<Movie[]> {
  const response = await fetch(`/api/movies${since ? `?since=${since.toISOString()}` : ''}`);
  if (!response.ok) {
    throw new Error(`Collection request failed with status ${response.status}`);
  }
  const movies = await response.json() as Movie[];
  return movies;
};

export async function getServerSeries(since?: Date): Promise<Series[]> {
  const response = await fetch(`/api/series${since ? `?since=${since.toISOString()}` : ''}`);
  if (!response.ok) {
    throw new Error(`Collection request failed with status ${response.status}`);
  }
  const series = await response.json() as Series[];
  return series;
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
