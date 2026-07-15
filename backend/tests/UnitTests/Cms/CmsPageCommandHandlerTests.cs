using CommerceHub.Modules.Cms.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommerceHub.Modules.Cms.UnitTests;

public class CmsPageCommandHandlerTests
{
    [Fact]
    public void CmsPage_ShouldInitializeWithDefaultValues()
    {
        var page = new CmsPage();

        page.IsPublished.Should().BeFalse();
        page.PublishedAt.Should().BeNull();
        page.Title.Should().BeEmpty();
        page.Slug.Should().BeEmpty();
    }

    [Fact]
    public void CmsPage_ShouldSetProperties_WhenCreated()
    {
        var page = new CmsPage
        {
            Title = "About Us",
            Slug = "about-us",
            Content = "<h1>About Us</h1><p>Our story</p>",
            MetaTitle = "About CommerceHub",
            MetaDescription = "Learn about CommerceHub",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow
        };

        page.Title.Should().Be("About Us");
        page.Slug.Should().Be("about-us");
        page.IsPublished.Should().BeTrue();
        page.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void CmsPage_ShouldSupportUnpublishing()
    {
        var page = new CmsPage
        {
            Title = "Temporary Page",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow
        };

        page.IsPublished = false;
        page.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void Banner_ShouldSetProperties()
    {
        var banner = new Banner
        {
            Title = "Summer Sale",
            Subtitle = "Up to 50% off",
            ImageUrl = "/images/summer-banner.jpg",
            LinkUrl = "/products/sale",
            SortOrder = 1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        banner.Title.Should().Be("Summer Sale");
        banner.ImageUrl.Should().Be("/images/summer-banner.jpg");
        banner.SortOrder.Should().Be(1);
    }

    [Fact]
    public void Banner_ShouldAllowNullSubtitleAndLink()
    {
        var banner = new Banner
        {
            Title = "Minimal Banner",
            ImageUrl = "/images/banner.jpg",
            SortOrder = 2
        };

        banner.Subtitle.Should().BeNull();
        banner.LinkUrl.Should().BeNull();
    }

    [Fact]
    public void Banner_ShouldSupportScheduling()
    {
        var now = DateTime.UtcNow;
        var banner = new Banner
        {
            Title = "Scheduled Banner",
            ImageUrl = "/images/scheduled.jpg",
            SortOrder = 3,
            StartDate = now.AddDays(1),
            EndDate = now.AddDays(7)
        };

        banner.StartDate.Should().BeAfter(now);
        banner.EndDate.Should().BeAfter(banner.StartDate.Value);
    }

    [Fact]
    public void Coupon_ShouldSetProperties()
    {
        var coupon = new global::CommerceHub.Modules.Cms.Domain.Entities.Coupon
        {
            Code = "CMS10",
            Type = "Percentage",
            DiscountPercentage = 10,
            MaxUsageCount = 100,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddMonths(1)
        };

        coupon.Code.Should().Be("CMS10");
        coupon.DiscountPercentage.Should().Be(10);
        coupon.MaxUsageCount.Should().Be(100);
        coupon.UsedCount.Should().Be(0);
    }

    [Fact]
    public void Coupon_ShouldTrackUsage()
    {
        var coupon = new global::CommerceHub.Modules.Cms.Domain.Entities.Coupon
        {
            Code = "USED50",
            Type = "Fixed",
            DiscountAmount = 50,
            MaxUsageCount = 10,
            UsedCount = 5
        };

        coupon.UsedCount.Should().Be(5);
        coupon.UsedCount++;
        coupon.UsedCount.Should().Be(6);
    }

    [Fact]
    public void RoleMenu_ShouldSetPermissions()
    {
        var roleMenu = new RoleMenu
        {
            RoleId = 1,
            MenuId = 1,
            CanView = true,
            CanCreate = true,
            CanEdit = false,
            CanDelete = false
        };

        roleMenu.CanView.Should().BeTrue();
        roleMenu.CanCreate.Should().BeTrue();
        roleMenu.CanEdit.Should().BeFalse();
        roleMenu.CanDelete.Should().BeFalse();
    }

    [Fact]
    public void RoleMenu_ShouldDefaultToViewOnly()
    {
        var roleMenu = new RoleMenu
        {
            RoleId = 1,
            MenuId = 1
        };

        roleMenu.CanView.Should().BeTrue();
        roleMenu.CanCreate.Should().BeFalse();
        roleMenu.CanEdit.Should().BeFalse();
        roleMenu.CanDelete.Should().BeFalse();
    }
}
