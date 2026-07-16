using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommerceHub.Infrastructure.Persistence;

public class DatabaseInitializer<TContext> : IDbInitializer where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ILogger<DatabaseInitializer<TContext>> _logger;

    public DatabaseInitializer(TContext context, ILogger<DatabaseInitializer<TContext>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var contextName = typeof(TContext).Name;
        try
        {
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database {Context} initialized", contextName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database {Context}", contextName);
            throw;
        }
    }
}
