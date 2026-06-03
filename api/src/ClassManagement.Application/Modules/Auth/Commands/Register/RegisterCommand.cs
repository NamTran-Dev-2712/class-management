using ClassManagement.Application.Common.Constants;
using MediatR;

public record RegisterCommand(
    string DisplayName,
    string Email,
    string UserName,
    string Password,
    string Role = ApplicationRoles.Student
) : IRequest<long>;
