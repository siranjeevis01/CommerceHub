namespace CommerceHub.Infrastructure.Persistence;

public interface IDbInitializer
{
    Task InitializeAsync();
}
