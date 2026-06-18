import { useContext } from 'react';
import { CollectionContext } from '../contexts';
import type { Collection } from '../types';

export default function useCollection(): Collection {
  return useContext(CollectionContext);
};
