using System.Security.Cryptography;
using ClassManagement.Application.Interfaces.Identity;

namespace ClassManagement.Infrastructure.Services.Identity;

// Generates a 16-char password with at least one upper, lower, digit and symbol, using a CSPRNG.
// Comfortably satisfies the configured Identity password policy. Characters are shuffled so the
// guaranteed-class characters aren't always in fixed positions.
public sealed class PasswordGenerator : IPasswordGenerator
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // no I/O
    private const string Lower = "abcdefghijkmnpqrstuvwxyz"; // no l/o
    private const string Digits = "23456789"; // no 0/1
    private const string Symbols = "!@#$%^&*?-_";
    private const string All = Upper + Lower + Digits + Symbols;
    private const int Length = 16;

    public string Generate()
    {
        var chars = new char[Length];

        // Guarantee one character from each class.
        chars[0] = Pick(Upper);
        chars[1] = Pick(Lower);
        chars[2] = Pick(Digits);
        chars[3] = Pick(Symbols);

        for (var i = 4; i < Length; i++)
            chars[i] = Pick(All);

        Shuffle(chars);
        return new string(chars);
    }

    private static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];

    private static void Shuffle(char[] chars)
    {
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
