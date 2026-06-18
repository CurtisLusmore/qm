import { CollectionContext } from '../contexts';
import type { Collection } from '../types';

export default function CollectionContextProvider({ children, value }: { children: React.ReactNode, value: Collection }): React.ReactElement {
  return (
    <CollectionContext.Provider value={value}>
      {children}
    </CollectionContext.Provider>
  );
};
