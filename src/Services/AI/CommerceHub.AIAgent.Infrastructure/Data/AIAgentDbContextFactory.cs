using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerceHub.AIAgent.Infrastructure.Data;

public class AIAgentDbContextFactory : IDesignTimeDbContextFactory<AIAgentDbContext>
{
    public AIAgentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AIAgentDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("AI_DB_CONNECTION")
            ?? "server=localhost;database=commercehub_ai;user=root;password=rootpassword";
        optionsBuilder.UseMySQL(connectionString);
        return new AIAgentDbContext(optionsBuilder.Options);
    }
}