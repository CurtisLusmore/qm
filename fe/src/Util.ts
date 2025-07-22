export default {
  FormatPercent(x: number): string { return x.toFixed(1) + '%'; },
  FormatBytes(bytes: number): string {
    bytes = +bytes;
    if (bytes < 1000) return `${bytes.toFixed(3)}B`;
    bytes /= 1000;
    if (bytes < 1000) return `${bytes.toFixed(3)}KB`;
    bytes /= 1000;
    if (bytes < 1000) return `${bytes.toFixed(3)}MB`;
    bytes /= 1000;
    if (bytes < 1000) return `${bytes.toFixed(3)}GB`;
    bytes /= 1000;
    return `${bytes}TB`;
  },
};
