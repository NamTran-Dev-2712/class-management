using System.Security.Cryptography;

namespace ClassManagement.Infrastructure.Services.Classroom;

// Crypto-random [A-Z0-9] invite code of a caller-supplied length (~36^len combinations — brute-force
// resistant per MVP-2 risk analysis). Length is live-configurable (system_settings.invite_code_length,
// clamped 6–12 by the policy). Uniqueness is ensured by the handler retrying against the repository.
public sealed class InviteCodeGenerator : IInviteCodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length)
    {
        var size = Math.Clamp(length, 6, 12);
        Span<char> chars = stackalloc char[size];
        for (var i = 0; i < size; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return new string(chars);
    }
}
