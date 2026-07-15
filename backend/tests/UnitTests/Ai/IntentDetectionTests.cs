using Xunit;
using CommerceHub.Modules.Ai.Application.Services;

namespace CommerceHub.Modules.Ai.UnitTests;

public class IntentDetectionTests
{
    [Theory]
    [InlineData("find me a red dress", "search")]
    [InlineData("i'm looking for shoes", "search")]
    [InlineData("search for laptops under 1000", "search")]
    [InlineData("recommend me some books", "recommendation")]
    [InlineData("what do you suggest", "recommendation")]
    [InlineData("show me trending products", "search")]
    [InlineData("where is my order", "order")]
    [InlineData("track my delivery", "order")]
    [InlineData("i want to return this", "search")]
    [InlineData("this product is broken", "complaint")]
    [InlineData("help me", "help")]
    [InlineData("how do i checkout", "order")]
    [InlineData("add to cart", "cart")]
    [InlineData("my cart items", "cart")]
    [InlineData("what is the price", "product_detail")]
    [InlineData("any discounts available", "pricing")]
    [InlineData("my account settings", "account")]
    [InlineData("change password", "account")]
    public void DetectIntent_ShouldReturnCorrectIntent(string message, string expectedIntent)
    {
        var detected = "general";
        foreach (var kvp in IntentPatterns.Patterns)
        {
            if (kvp.Value.Any(p => message.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                detected = kvp.Key;
                break;
            }
        }
        Assert.Equal(expectedIntent, detected);
    }

    [Fact]
    public void DetectIntent_UnknownMessage_ShouldReturnGeneral()
    {
        var message = "the sky is blue";
        var detected = "general";
        foreach (var kvp in IntentPatterns.Patterns)
        {
            if (kvp.Value.Any(p => message.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                detected = kvp.Key;
                break;
            }
        }
        Assert.Equal("general", detected);
    }

    [Fact]
    public void IntentPatterns_ShouldContainAllCategories()
    {
        var expectedCategories = new[] { "search", "recommendation", "order", "product_detail", "cart", "help", "complaint", "pricing", "account", "seller" };
        foreach (var category in expectedCategories)
        {
            Assert.Contains(category, IntentPatterns.Patterns.Keys);
        }
    }
}