using System.Security.Cryptography;

namespace ClassManagement.Infrastructure.Services.Classroom;

// Crypto-random 8-char [A-Z0-9] invite code (~2.8 trillion combinations — brute-force resistant per
// MVP-2 risk analysis). Uniqueness is ensured by the handler retrying against the repository.
public sealed class InviteCodeGenerator : IInviteCodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int Length = 8;

    public string Generate()
    {
        Span<char> chars = stackalloc char[Length];
        for (var i = 0; i < Length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return new string(chars);
    }
}
