import { createContext, useEffect, useReducer } from 'react';
import { KeyValueStorePromise } from '../clients';
import type { Collection, CollectionStatus, Movie, Series, Title, TitleSummary } from '../types';
import type { KeyValueStore } from '@fifteenthstandard/storage';

export const CollectionContext = createContext<Collection>({} as Collection);

type State = {
  loaded: boolean;
  store: KeyValueStore | undefined;
  movies: Record<string, CollectionStatus<Movie>>;
  series: Record<string, CollectionStatus<Series>>;
};

const initialState = {
  loaded: false,
  store: undefined,
  movies: {} as Record<string, CollectionStatus<Movie>>,
  series: {} as Record<string, CollectionStatus<Series>>,
};

type InitializeAction = {
  type: 'initialize';
  store: KeyValueStore | undefined;
  movies: CollectionStatus<Movie>[];
  series: CollectionStatus<Series>[];
};

type AddAction = {
  type: 'add';
  title: CollectionStatus<Title>;
};

type RemoveAction = {
  type: 'remove';
  titleId: string;
};

type UpdateAction = {
  type: 'update';
  title: CollectionStatus<Title>;
};

type Action =
  | InitializeAction
  | AddAction
  | RemoveAction
  | UpdateAction;

function initialize(state: State, store: KeyValueStore | undefined, movies: CollectionStatus<Movie>[], series: CollectionStatus<Series>[]): State {
  const newState = { ...state, store, loaded: true };
  movies.forEach(t => newState.movies[t.id] = t);
  series.forEach(t => newState.series[t.id] = t);
  return newState;
};

function addTitle(state: State, title: CollectionStatus<Title>): State {
  switch (title.type) {
    case 'movie':
      return {
        ...state,
        movies: {
          ...state.movies,
          [title.id]: title as CollectionStatus<Movie>,
        },
      };

    case 'series':
      return {
        ...state,
        series: {
          ...state.series,
          [title.id]: title as CollectionStatus<Series>,
        },
      };

    default:
      return state;
  }
};

function removeTitle(state: State, titleId: string): State {
  if (state.movies.hasOwnProperty(titleId)) {
    const newMovies = { ...state.movies };
    delete newMovies[titleId];
    return {
      ...state,
      movies: newMovies,
    };
  } else if (state.series.hasOwnProperty(titleId)) {
    const newSeries = { ...state.series };
    delete newSeries[titleId];
    return {
      ...state,
      series: newSeries,
    };
  } else {
    return state;
  }
};

function update(state: State, title: CollectionStatus<Title>): State {
  switch (title.type) {
    case 'movie':
      return {
        ...state,
        movies: {
          ...state.movies,
          [title.id]: title as CollectionStatus<Movie>,
        },
      };

    case 'series':
      return {
        ...state,
        series: {
          ...state.series,
          [title.id]: title as CollectionStatus<Series>,
        },
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

  useEffect(() => {
    (async function initialize() {
      const store = await KeyValueStorePromise;
      const [ movies, series ] = await Promise.all([
        store.list<CollectionStatus<Movie>>('movie').all() as Promise<CollectionStatus<Movie>[]>,
        store.list<CollectionStatus<Series>>('series').all() as Promise<CollectionStatus<Series>[]>,
      ]);
      dispatch({ type: 'initialize', store, movies, series });
    }());
  }, []);

  const movies = Object.values(state.movies).toSorted((a, b) => a.name.localeCompare(b.name));
  const series = Object.values(state.series).toSorted((a, b) => a.name.localeCompare(b.name));
  const recentlyAdded = [...movies, ...series]
    .filter(item => item.addedOn !== undefined)
    .toSorted((a, b) => b.addedOn!.getTime() - a.addedOn!.getTime())
    .slice(0, 3);

  function get(titleId: string): CollectionStatus<Title> | undefined {
    if (state.movies.hasOwnProperty(titleId)) {
      return state.movies[titleId];
    } else if (state.series.hasOwnProperty(titleId)) {
      return state.series[titleId];
    } else {
      return undefined;
    }
  };

  async function add(title: Title): Promise<void> {
    const collectionItem: CollectionStatus<Title> = {
      ...title,
      inCollection: true,
      addedOn: new Date(),
      watched: false,
      downloadStatus: 'not_downloaded',
    };
    dispatch({ type: 'add', title: collectionItem });
    await state.store!.put(title.type, title.id, collectionItem);
  };

  async function remove(titleId: string): Promise<void> {
    dispatch({ type: 'remove', titleId });
    const title = get(titleId);
    if (title) await state.store!.remove(title.type, titleId);
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

    function checkOne(searchResult: T): CollectionStatus<T> {
      const title = get(searchResult.id);
      return title === undefined
        ? { ...searchResult, inCollection: false, downloadStatus: 'not_downloaded' } as CollectionStatus<T>
        : { ...title, ...searchResult } as CollectionStatus<T>;
    }
  };

  return {
    loaded: state.loaded,
    movies,
    series,
    recentlyAdded,
    get,
    add,
    remove,
    markWatched,
    check,
  };
};
