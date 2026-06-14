public record TokenResult(
    string AccessToken,
    string RefreshToken,
    string RefreshTokenHash,
    DateTime AccessTokenExpiry,
    DateTime RefreshTokenExpiry
);
