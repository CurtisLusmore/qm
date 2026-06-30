import type { Episode, Movie, Series, Title } from '../types';

type Data = {
  data: {
    title: TitleResult;
  };
}

type TitleResult = {
  id: string;
  titleText: { text: string };
  plot?: { plotText: { plainText: string } };
  genres: { genres: { text: string }[] };
  primaryImage: { url: string };
  latestTrailer?: {
    name: { value: string };
    playbackURLs: { url: string }[];
  };
  releaseYear?: { year: number; endYear?: number };
  releaseDate?: { day: number; month: number; year: number };
  titleType: { text: string };
  series?: {
    series: { id: string; titleText: { text: string } };
    episodeNumber: { seasonNumber: number; episodeNumber: number };
  };
  episodes?: {
    episodes: {
      pageInfo: { hasNextPage: boolean; endCursor: string };
      edges: {
        node: TitleResult;
      }[];
    };
  };
  runtime?: { seconds: number };
  certificate?: { rating: string; ratingReason: string };
  ratingsSummary: { aggregateRating: number; voteCount: number };
  CAST: {
    edges: {
      node: {
        name: { id: string; nameText: { text: string } };
      };
    }[];
  };
  DIRECTORS: {
    edges: {
      node: {
        name: { id: string; nameText: { text: string } };
      };
    }[];
  };
  WRITERS: {
    edges: {
      node: {
        name: { id: string; nameText: { text: string } };
      };
    }[];
  };
}

function mapTitleResultToEpisode(result: TitleResult): Episode {
  return {
    id: result.id,
    name: result.titleText.text,
    plot: result.plot?.plotText.plainText ?? '',
    genres: result.genres.genres.map(genre => genre.text),
    imageUrl: result.primaryImage.url,
    trailerUrl: result.latestTrailer?.playbackURLs[0]?.url,
    year: result.releaseYear?.year,
    endYear: result.releaseYear?.endYear,
    releaseDate: result.releaseDate && new Date(result.releaseDate.year, result.releaseDate.month - 1, result.releaseDate.day),
    type: 'episode',
    seasonNumber: result.series?.episodeNumber ? result.series.episodeNumber.seasonNumber : 0,
    episodeNumber: result.series?.episodeNumber ? result.series.episodeNumber.episodeNumber : 0,
    runtimeSeconds: result.runtime?.seconds,
    classification: result.certificate?.rating || 'N/A',
    ratings: {
      rating: result.ratingsSummary.aggregateRating,
      count: result.ratingsSummary.voteCount,
    },
    cast: result.CAST.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
    directors: result.DIRECTORS.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
    writers: result.WRITERS.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
  };
};

function mapTitleResultToMovie(result: TitleResult): Movie {
  return {
    id: result.id,
    type: 'movie',
    name: result.titleText.text,
    plot: result.plot?.plotText.plainText ?? '',
    genres: result.genres.genres.map(genre => genre.text),
    imageUrl: result.primaryImage.url,
    trailerUrl: result.latestTrailer?.playbackURLs[0]?.url,
    year: result.releaseYear?.year,
    endYear: result.releaseYear?.endYear,
    releaseDate: result.releaseDate && new Date(result.releaseDate.year, result.releaseDate.month - 1, result.releaseDate.day),
    runtimeSeconds: result.runtime?.seconds,
    classification: result.certificate?.rating || 'N/A',
    ratings: {
      rating: result.ratingsSummary.aggregateRating,
      count: result.ratingsSummary.voteCount,
    },
    cast: result.CAST.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
    directors: result.DIRECTORS.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
    writers: result.WRITERS.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
  };
};

function mapTitleResultToSeries(result: TitleResult): Series {
  return {
    id: result.id,
    type: 'series',
    name: result.titleText.text,
    plot: result.plot?.plotText.plainText ?? '',
    genres: result.genres.genres.map(genre => genre.text),
    imageUrl: result.primaryImage.url,
    trailerUrl: result.latestTrailer?.playbackURLs[0]?.url,
    year: result.releaseYear?.year,
    endYear: result.releaseYear?.endYear,
    releaseDate: result.releaseDate && new Date(result.releaseDate.year, result.releaseDate.month - 1, result.releaseDate.day),
    episodes: [],
    runtimeSeconds: result.runtime?.seconds,
    classification: result.certificate?.rating || 'N/A',
    ratings: {
      rating: result.ratingsSummary.aggregateRating,
      count: result.ratingsSummary.voteCount,
    },
    cast: result.CAST.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
    directors: result.DIRECTORS.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
    writers: result.WRITERS.edges.map(edge => ({
      id: edge.node.name.id,
      name: edge.node.name.nameText.text,
    })),
  };
};

export default async function getTitle(id: string, includeEpisodes: boolean = true): Promise<Title> {
  let result = await query(id);

  switch (result.titleType.text) {
    case 'Movie':
    case 'TV Movie':
      return mapTitleResultToMovie(result);

    case 'TV Episode':
      return mapTitleResultToEpisode(result);

    case 'TV Mini Series':
    case 'TV Series':
      const series = mapTitleResultToSeries(result);

      const episodes: Episode[] = [];
      let endCursor: string;
      while (includeEpisodes) {
        const episodeResults = result.episodes?.episodes.edges || [];
        episodes.push(...episodeResults.filter(edge => edge.node.runtime).map(edge => mapTitleResultToEpisode(edge.node)));
        endCursor = result.episodes?.episodes.pageInfo.endCursor || '';
        if (result.episodes?.episodes.pageInfo.hasNextPage) {
          result = await query(id, endCursor);
        } else {
          break;
        } 
      }
      series.episodes = episodes;
      return series;

    default:
      throw new Error(`Unsupported title type: ${result.titleType.text}`);
  }

  async function query(id: string, endCursor: string = ''): Promise<TitleResult> {
    let resp = await fetch(
     `/api/proxy?url=https://graphql.imdb.com`,
      {
        'method': 'POST',
        'headers': { 'Content-Type': 'application/json' },
        'body': JSON.stringify({ query: 
          `query {
            title(id: "${id}") {
              id
              titleText { text }
              plot { plotText { plainText } }
              genres { genres { text } }
              primaryImage { url }
              latestTrailer { playbackURLs { url } }
              releaseYear { year endYear }
              releaseDate { day month year }
              titleType { text }
              series {
                series { id titleText { text } }
                episodeNumber { seasonNumber episodeNumber }
              }
              episodes {
                episodes(first: 250, after: "${endCursor}") {
                  pageInfo { hasNextPage endCursor }
                  edges {
                    node {
                      id
                      titleText { text }
                      plot { plotText { plainText } }
                      genres { genres { text } }
                      primaryImage { url }
                      latestTrailer { playbackURLs { url } }
                      releaseYear { year endYear }
                      releaseDate { day month year }
                      titleType { text }
                      series {
                        series { id titleText { text } }
                        episodeNumber { seasonNumber episodeNumber }
                      }
                      runtime { seconds }
                      certificate { rating ratingReason }
                      ratingsSummary { aggregateRating voteCount }
                      CAST: credits(first: 10, filter: { categories: ["actor", "actress"] }) {
                        edges {
                          node {
                            name { id nameText { text } }
                          }
                        }
                      }
                      DIRECTORS: credits(first: 10, filter: { categories: ["director"] }) {
                        edges {
                          node {
                            name { id nameText { text } }
                          }
                        }
                      }
                      WRITERS: credits(first: 10, filter: { categories: ["writer"] }) {
                        edges {
                          node {
                            name { id nameText { text } }
                          }
                        }
                      }
                    }
                  }
                }
              }
              runtime { seconds }
              certificate { rating ratingReason }
              ratingsSummary { aggregateRating voteCount }
              CAST: credits(first: 10, filter: { categories: ["actor", "actress"] }) {
                edges {
                  node {
                    name { id nameText { text } }
                  }
                }
              }
              DIRECTORS: credits(first: 10, filter: { categories: ["director"] }) {
                edges {
                  node {
                    name { id nameText { text } }
                  }
                }
              }
              WRITERS: credits(first: 10, filter: { categories: ["writer"] }) {
                edges {
                  node {
                    name { id nameText { text } }
                  }
                }
              }
            }
          }`
        }),
      }
    );
    return (await resp.json() as Data).data.title;
  };
};
