using CommerceHub.Modules.Cms.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Modules.Cms.UnitTests;

public class CmsDomainTests
{
    [Fact]
    public void Campaign_ShouldSetProperties()
    {
        var campaign = new Campaign
        {
            Name = "Summer Sale",
            Type = "Seasonal",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            DiscountPercentage = 20.0m,
            Description = "Summer promotion"
        };

        campaign.Name.Should().Be("Summer Sale");
        campaign.DiscountPercentage.Should().Be(20.0m);
    }

    [Fact]
    public void Campaign_ShouldSupportFixedDiscount()
    {
        var campaign = new Campaign
        {
            Name = "Clearance",
            Type = "Clearance",
            FixedDiscount = 500.00m
        };

        campaign.FixedDiscount.Should().Be(500.00m);
    }

    [Fact]
    public void Menu_ShouldSetProperties()
    {
        var menu = new Menu
        {
            Name = "Products",
            Route = "/products",
            Icon = "shopping-bag",
            DisplayOrder = 1,
            IsVisible = true
        };

        menu.Name.Should().Be("Products");
        menu.Route.Should().Be("/products");
        menu.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void Menu_ShouldSupportNestedStructure()
    {
        var parent = new Menu { Name = "Categories", Route = "/categories" };
        var child = new Menu { Name = "Electronics", Route = "/categories/electronics", ParentMenu = parent, ParentMenuId = parent.Id };

        child.ParentMenuId.Should().Be(parent.Id);
    }

    [Fact]
    public void PlatformSetting_ShouldStoreKeyValue()
    {
        var setting = new PlatformSetting
        {
            Key = "SiteName",
            Value = "CommerceHub",
            Description = "Main site name"
        };

        setting.Key.Should().Be("SiteName");
        setting.Value.Should().Be("CommerceHub");
    }

    [Fact]
    public void FeatureToggle_ShouldControlFeatures()
    {
        var toggle = new FeatureToggle
        {
            Key = "NewCheckout",
            Enabled = true,
            Description = "Enable new checkout flow"
        };

        toggle.Enabled.Should().BeTrue();
    }

    [Fact]
    public void FeatureToggle_ShouldBeDisabledByDefault()
    {
        var toggle = new FeatureToggle { Key = "ExperimentalFeature" };
        toggle.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Menu_ShouldHaveSubMenus()
    {
        var parent = new Menu { Name = "Main" };
        parent.SubMenus.Add(new Menu { Name = "Sub 1" });
        parent.SubMenus.Add(new Menu { Name = "Sub 2" });

        parent.SubMenus.Should().HaveCount(2);
    }
}
