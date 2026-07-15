using CommerceHub.Modules.Cms.Domain.Entities;
using CommerceHub.Modules.Cms.Infrastructure.Data;
using CommerceHub.TestBase;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommerceHub.Modules.Cms.IntegrationTests;

public class CmsDbContextTests : IntegrationTestBase
{
    [Fact]
    public async Task CanCreateAndRetrieveCampaign()
    {
        var options = new DbContextOptionsBuilder<CmsDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new CmsDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var campaign = new Campaign
        {
            Name = "Summer Sale",
            Type = "Seasonal",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Description = "Summer promotion campaign"
        };

        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        var retrieved = await context.Campaigns.FirstOrDefaultAsync(c => c.Name == "Summer Sale");
        retrieved.Should().NotBeNull();
        retrieved!.Type.Should().Be("Seasonal");
    }

    [Fact]
    public async Task CanCreateMenu()
    {
        var options = new DbContextOptionsBuilder<CmsDbContext>()
            .UseMySQL(MySqlConnectionString)
            .Options;

        using var context = new CmsDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var menu = new Menu
        {
            Name = "Main Menu",
            Route = "/",
            DisplayOrder = 1,
            IsVisible = true
        };

        context.Menus.Add(menu);
        await context.SaveChangesAsync();

        var retrieved = await context.Menus.FirstOrDefaultAsync(m => m.Name == "Main Menu");
        retrieved.Should().NotBeNull();
        retrieved!.Route.Should().Be("/");
    }
}
