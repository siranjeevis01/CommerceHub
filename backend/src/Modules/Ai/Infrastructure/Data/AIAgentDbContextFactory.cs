using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.Modules.Ai.Infrastructure.Data;

public class AIAgentDbContextFactory : IDesignTimeDbContextFactory<AIAgentDbContext>
{
    public AIAgentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AIAgentDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("AI_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_ai;user=root;password=rootpassword";
        if (!connectionString.Contains("SslMode")) connectionString += ";SslMode=Required;AllowPublicKeyRetrieval=true";
        optionsBuilder.UseMySQL(connectionString);
        return new AIAgentDbContext(optionsBuilder.Options);
    }
}
