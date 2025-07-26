export default {
  FormatPercent(x: number): string { return x.toFixed(1) + '%'; },
  FormatBytes(bytes: number): string {
    bytes = +bytes;
    if (bytes < 1000) return `${Math.floor(bytes)}B`;
    bytes /= 1000;
    if (bytes < 1000) return `${Math.floor(bytes)}KB`;
    bytes /= 1000;
    if (bytes < 1000) return `${Math.floor(bytes)}MB`;
    bytes /= 1000;
    if (bytes < 1000) return `${Math.floor(bytes)}GB`;
    bytes /= 1000;
    return `${bytes}TB`;
  },
};
