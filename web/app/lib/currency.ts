/** Format a VND amount (integer đồng, no decimals) for the active locale. */
export function formatVnd(amount: number, locale = "vi-VN"): string {
    return new Intl.NumberFormat(locale === "vi" ? "vi-VN" : locale, {
        style: "currency",
        currency: "VND",
        maximumFractionDigits: 0,
    }).format(amount);
}
