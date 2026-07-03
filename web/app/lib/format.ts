/** Human-readable byte size (e.g. 1536 → "1.5 KB"). 0 stays "0 B". */
export function formatBytes(bytes: number, fractionDigits = 1): string {
    if (!bytes || bytes <= 0) return "0 B";
    const units = ["B", "KB", "MB", "GB", "TB"];
    const exponent = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1);
    const value = bytes / 1024 ** exponent;
    const rounded = exponent === 0 ? value : Number(value.toFixed(fractionDigits));
    return `${rounded} ${units[exponent]}`;
}
