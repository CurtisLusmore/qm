export default function createServerEventSource(): EventSource {
  return new EventSource('/api/subscribe');
};
