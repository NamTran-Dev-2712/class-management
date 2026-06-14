/**
 * Static, build-time feature flags. Read from `import.meta.env` so they can be
 * toggled per environment without code changes. Extend as features land.
 */
export const featureFlags = {
    aiChat: import.meta.env.VITE_FEATURE_AI_CHAT === "true",
    payments: import.meta.env.VITE_FEATURE_PAYMENTS === "true",
} as const;

export type FeatureFlag = keyof typeof featureFlags;

export const isFeatureEnabled = (flag: FeatureFlag): boolean => featureFlags[flag];
