using ClassManagement.Application.Common.Constants;
using Hangfire.Dashboard;

namespace ClassManagement.Api.Security;

// Restricts the Hangfire dashboard to authenticated Admin users. Authentication runs in the
// pipeline (cookie/JWT) before this filter, so HttpContext.User is already populated.
public sealed class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(ApplicationRoles.Admin);
    }
}
