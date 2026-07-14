import { createContext, useEffect, useReducer } from 'react';
import type {
  DownloadTracker,
  ServerEvent,
  ServerEventHandlerRegistration,
} from '../types';

export const DownloadsContext = createContext<DownloadTracker[]>({} as DownloadTracker[]);

function reduce(state: DownloadTracker[], action: ServerEvent): DownloadTracker[] {
  switch (action.type) {
    case 'DownloadAdded':
      return [...state, action.download];
    case 'DownloadProgress':
      return updateOrAdd(action.download);
    case 'DownloadCompleted':
      return updateOrAdd(action.download);
    case 'DownloadRemoved':
      return state.filter(download => download.infoHash !== action.infoHash);
    case 'DownloadFailed':
      return updateOrAdd(action.download);
    default:
      return state;
  }

  function updateOrAdd(download: DownloadTracker): DownloadTracker[] {
    const newState = [ ...state ];
    for (let i = 0; i < newState.length; i++) {
      if (newState[i].infoHash === download.infoHash) {
        newState[i] = download;
        return newState;
      }
    }
    return [ ...state, download ];
  };
};

export function createDownloadsContext(registration: ServerEventHandlerRegistration): DownloadTracker[] {
  const [ downloads, dispatch ] = useReducer(reduce, [] as DownloadTracker[]);

  useEffect(() => {
    return registration(dispatch);
  }, [ registration ]);

  return downloads;
};
