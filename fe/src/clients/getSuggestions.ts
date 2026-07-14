import type { TitleSummary, TitleType } from '../types';

type SearchResults = {
  d: [
    {
      i?: {
        height: number;
        imageUrl: string;
        width: number;
      },
      id: string;
      l: string;
      q: string;
      qid: 'movie' | 'tvMiniSeries' | 'tvMovie' | 'tvSeries';
      rank: number;
      s: string;
      y?: number;
    },
  ],
}

function mapSearchResults(results: SearchResults): TitleSummary[] {
  return results.d
    .filter(result => ['movie', 'tvSeries', 'tvMovie', 'tvMiniSeries'].includes(result.qid))
    .filter(result => result.i)
    .map(result => ({
      id: result.id,
      type: mapQidToType(result.qid),
      name: result.l,
      year: result.y,
      imageUrl: result.i!.imageUrl,
    })
  );

  function mapQidToType(qid: string): TitleType {
    switch (qid) {
      case 'movie':
      case 'tvMovie':
        return 'Movie';
      case 'tvMiniSeries':
      case 'tvSeries':
        return 'Series';
      default:
        throw new Error(`Unknown qid: ${qid}`);
    }
  };
};

export default async function getSuggestions(searchTerm: string): Promise<TitleSummary[]> {
  if (!searchTerm) return [];
  const resp = await fetch(`/api/proxy?url=https://v3.sg.media-imdb.com/suggestion/x/${encodeURIComponent(searchTerm)}.json`);
  const data = await resp.json() as SearchResults;
  return mapSearchResults(data);
};
