namespace ClassManagement.Application.Interfaces.Identity;

/// <summary>
/// Generates a cryptographically strong password that satisfies the Identity password policy
/// (used when an admin creates a user — the temporary password is emailed, never chosen).
/// </summary>
public interface IPasswordGenerator
{
    string Generate();
}
