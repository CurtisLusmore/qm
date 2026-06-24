export default function createServerEventSource(): EventSource {
  return new EventSource('http://localhost:5138/subscribe');
};
