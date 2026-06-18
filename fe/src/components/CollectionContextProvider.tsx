import { createCollectionContext, CollectionContext } from '../contexts';

export default function CollectionContextProvider({ children }: { children: React.ReactNode }): React.ReactElement {
  const value = createCollectionContext();
  return (
    <CollectionContext.Provider value={value}>
      {children}
    </CollectionContext.Provider>
  );
};
