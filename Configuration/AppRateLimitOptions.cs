namespace CommerceHub.API.Configuration;

public class AppRateLimitOptions
{
    public int MaxRequestsPerMinute { get; set; } = 100;
    public int MaxRequestsPerHour { get; set; } = 1000;
}