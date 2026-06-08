import type { ProfileResponse } from "@/services/auth/dtos/queries/profile/profile.response";

/** Login sets the auth cookie server-side and returns the authenticated user. */
export type LoginResponse = ProfileResponse;
