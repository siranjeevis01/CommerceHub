using Hangfire.Dashboard;

namespace CommerceHub.ServiceDefaults;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.Request.Host.Host is "localhost" or "127.0.0.1")
            return true;

        var user = httpContext.User;
        return user.Identity?.IsAuthenticated == true &&
               user.IsInRole("Admin");
    }
}
