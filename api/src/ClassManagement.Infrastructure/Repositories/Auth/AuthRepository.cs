using ClassManagement.Application.Common.Constants;
using ClassManagement.Application.Exceptions;
using Microsoft.AspNetCore.Identity;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public AuthRepository(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> RegisterAsync(
        string fullName,
        string email,
        string password,
        string role = ApplicationRoles.Student,
        CancellationToken cancellationToken = default
    )
    {
        // Check for existing user by email
        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
            throw new ConflictException("Email is already registered.");

        var user = new ApplicationUser
        {
            DisplayName = fullName,
            Email = email,
            UserName = email,
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            throw new BadException("Registration failed.");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            // Roll back: clean up the created user to keep data consistent
            await _userManager.DeleteAsync(user);

            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            throw new BadException("Failed to assign role.");
        }

        return user.Id;
    }
}
