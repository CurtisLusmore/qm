import { createContext, useCallback, useEffect, useMemo, useReducer, useState } from 'react';
import type { KeyValueStore } from '@fifteenthstandard/storage';
import type {
  Collection,
  Movie,
  Series,
  ServerCollection,
  Title,
  TitleSummary,
} from '../types';
import {
  addMovieToServerCollection,
  addSeriesToServerCollection,
  getServerCollection,
  KeyValueStorePromise,
  removeMovieFromServerCollection,
  removeSeriesFromServerCollection,
} from '../clients';

export const CollectionContext = createContext<Collection>({} as Collection);

type State = {
  loaded: boolean;
  store: KeyValueStore | undefined;
  movies: Movie[];
  series: Series[];
};

const initialState = {
  loaded: false,
  store: undefined,
  movies: [] as Movie[],
  series: [] as Series[],
};

type InitializeAction = {
  type: 'initialize';
  store: KeyValueStore | undefined;
  movies: Movie[];
  series: Series[];
};

type AddAction = {
  type: 'add';
  title: Title;
};

type RemoveAction = {
  type: 'remove';
  titleId: string;
};

type UpdateAction = {
  type: 'update';
  title: Title;
};

type Action =
  | InitializeAction
  | AddAction
  | RemoveAction
  | UpdateAction;

function sorted<T extends Title>(items: T[]): T[] {
  return items.toSorted((a, b) => a.name.localeCompare(b.name));
}

function initialize(state: State, store: KeyValueStore | undefined, movies: Movie[], series: Series[]): State {
  const newState = {
    ...state,
    loaded: true,
    store,
    movies: sorted(movies),
    series: sorted(series)
  };
  return newState;
};

function addTitle(state: State, title: Title): State {
  switch (title.type) {
    case 'movie':
      return {
        ...state,
        movies: sorted([...state.movies, title as Movie]),
      };

    case 'series':
      return {
        ...state,
        series: sorted([...state.series, title as Series]),
      };

    default:
      return state;
  }
};

function removeTitle(state: State, titleId: string): State {
  if (state.movies.some(item => item.id === titleId)) {
    const newMovies = state.movies.filter(item => item.id !== titleId);
    return {
      ...state,
      movies: newMovies,
    };
  } else if (state.series.some(item => item.id === titleId)) {
    const newSeries = state.series.filter(item => item.id !== titleId);
    return {
      ...state,
      series: newSeries,
    };
  } else {
    return state;
  }
};

function update(state: State, title: Title): State {
  switch (title.type) {
    case 'movie':
      return {
        ...state,
        movies: sorted([...state.movies, title as Movie]),
      };

    case 'series':
      return {
        ...state,
        series: sorted([...state.series, title as Series]),
      };

    default:
      return state;
  }
};

function reduce(state: State, action: Action): State {
  switch (action.type) {
    case 'initialize': {
      const { store, movies, series } = action;
      return initialize(state, store, movies, series);
    }

    case 'add': {
      const { title } = action;
      return addTitle(state, title);
    }

    case 'remove': {
      const { titleId } = action;
      return removeTitle(state, titleId);
    }

    case 'update': {
      const { title } = action;
      return update(state, title);
    }

    default:
      return state;
  }
};

export function createCollectionContext(): Collection {
  const [ state, dispatch ] = useReducer(reduce, initialState);
  const [ serverState, setServerState ] = useState<ServerCollection>({ movies: [] as Movie[], series: [] as Series[] });

  useEffect(() => {
    (async function () {
      const collection = await getServerCollection();
      setServerState(collection);
    }());
  }, []);

  useEffect(() => {
    for (const movie of serverState.movies) {
      updateFromServer(movie);
    }
  }, [ serverState.movies ]);

  useEffect(() => {
    for (const series of serverState.series) {
      updateFromServer(series);
    }
  }, [ serverState.series ]);

  useEffect(() => {
    (async function initialize() {
      const store = await KeyValueStorePromise;
      const [ movies, series ] = await Promise.all([
        store.list<Movie>('Movie').all() as Promise<Movie[]>,
        store.list<Series>('Series').all() as Promise<Series[]>,
      ]);
      dispatch({ type: 'initialize', store, movies, series });
    }());
  }, []);

  const recentlyAdded = useMemo(() => [ ...state.movies, ...state.series ]
    .filter(item => item.addedOn !== undefined)
    .toSorted((a, b) => b.addedOn!.getTime() - a.addedOn!.getTime())
    .slice(0, 10), [ state.movies, state.series ]);

  function get(titleId: string): Title | undefined {
    const movie = state.movies.find(item => item.id === titleId);
    if (movie) return movie;
    const series = state.series.find(item => item.id === titleId);
    if (series) return series;
    return undefined;
  };

  async function add(title: Title): Promise<void> {
    const collectionItem: Title = {
      ...title,
      inCollection: true,
      addedOn: new Date(),
      watched: false,
      lastWatched: undefined,
      downloaded: false,
    };
    await state.store!.put(title.type, title.id, collectionItem);
    await (title.type === 'movie'
      ? addMovieToServerCollection(title as Movie)
      : addSeriesToServerCollection(title as Series)
    );
    dispatch({ type: 'add', title: collectionItem });
  };

  async function updateFromServer(title: Title): Promise<void> {
    title = {
      inCollection: true,
      addedOn: new Date(),
      watched: false,
      lastWatched: undefined,
      downloaded: false,
      ...title,
    };
    await state.store!.put(title.type, title.id, title);
    dispatch({ type: 'update', title });
  };

  async function remove(titleId: string): Promise<void> {
    const title = get(titleId);
    if (title) {
      await state.store!.remove(title.type, titleId);
      await (title.type === 'movie'
        ? removeMovieFromServerCollection(titleId)
        : removeSeriesFromServerCollection(titleId)
    );
    }
    dispatch({ type: 'remove', titleId });
  };

  async function markWatched(titleId: string): Promise<void> {
    const existing = get(titleId);
    if (!existing) return;
    const title = {
      ...existing,
      watched: true,
      lastWatched: new Date(),
    };
    dispatch({ type: 'update', title });
    await state.store!.put(title.type, titleId, title);
  };

  function check<T extends TitleSummary>(searchResults: T | T[]): any {
    return Array.isArray(searchResults)
      ? searchResults.map(checkOne)
      : checkOne(searchResults);

    function checkOne(searchResult: T): T {
      const title = get(searchResult.id);
      return title === undefined
        ? { ...searchResult, inCollection: false, addedOn: undefined, watched: false, lastWatched: undefined, downloaded: false } as T
        : { ...searchResult, ...title } as T;
    }
  };

  const getMemoized = useCallback(get, [ state.movies, state.series ]);
  const addMemoized = useCallback(add, [ state.store ]);
  const removeMemoized = useCallback(remove, [ state.store ]);
  const markWatchedMemoized = useCallback(markWatched, [ state.store ]);
  const checkMemoized = useCallback(check, [ state.movies, state.series ]);

  const contextValue = useMemo(() => ({
    loaded: state.loaded,
    movies: state.movies,
    series: state.series,
    recentlyAdded,
    get: getMemoized,
    add: addMemoized,
    remove: removeMemoized,
    markWatched: markWatchedMemoized,
    check: checkMemoized,
  }), [ state.loaded, state.movies, state.series, recentlyAdded ]);

  return contextValue;
};
