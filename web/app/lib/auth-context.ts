import { createContext } from "react-router";

import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

/**
 * Router context carrying the authenticated user for the current request.
 * Populated once by the root server middleware and read by route loaders/guards.
 */
export const userContext = createContext<ProfileResponse | null>(null);
