using CommerceHub.Modules.Ai.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.Modules.Ai.Application.Common.Interfaces;

public interface IAIAgentDbContext
{
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<ProductRecommendation> ProductRecommendations { get; }
    DbSet<SearchQuery> SearchQueries { get; }
    DbSet<AISession> AISessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
