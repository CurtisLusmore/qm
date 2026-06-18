import { IndexedDbKeyValueStore, type KeyValueStore } from '@fifteenthstandard/storage';

const KeyValueStorePromise: Promise<KeyValueStore> = (async function () {
  const result = indexedDB.open('qm', 1);
  
  return new Promise((resolve, reject) => {
    result.onupgradeneeded = () => {
      const db = result.result;
      if (!db.objectStoreNames.contains('collection')) {
        db.createObjectStore('collection');
      }
    };
    result.onsuccess = () => resolve(new IndexedDbKeyValueStore(result.result, 'collection'));
    result.onerror = () => reject(result.error);
  });
}());

export default KeyValueStorePromise;
