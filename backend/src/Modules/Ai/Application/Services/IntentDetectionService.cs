namespace CommerceHub.Modules.Ai.Application.Services;

public static class IntentPatterns
{
    public static readonly Dictionary<string, string[]> Patterns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["search"] = new[] { "search", "find", "looking for", "show me", "i want", "need", "where can i", "browse", "explore", "got any" },
        ["recommendation"] = new[] { "recommend", "suggest", "what should", "what do you", "similar", "like this", "alternatives", "best", "top", "trending", "popular" },
        ["order"] = new[] { "order", "my orders", "track", "delivery", "shipping", "ordered", "purchase", "buy", "place order", "checkout" },
        ["product_detail"] = new[] { "tell me about", "details", "description", "specifications", "features", "what is", "reviews", "rating" },
        ["cart"] = new[] { "cart", "add to cart", "remove from cart", "my cart", "checkout", "bag" },
        ["help"] = new[] { "help", "how to", "how do i", "guide", "tutorial", "what can you", "capabilities", "support" },
        ["complaint"] = new[] { "complaint", "issue", "problem", "not working", "broken", "defective", "damaged", "wrong", "return", "refund", "cancel" },
        ["pricing"] = new[] { "price", "cost", "cheap", "expensive", "budget", "discount", "offer", "deal", "sale", "coupon", "promo" },
        ["account"] = new[] { "account", "profile", "settings", "password", "login", "logout", "sign in", "sign up", "register" },
        ["seller"] = new[] { "seller", "vendor", "store", "shop", "brand" }
    };
}
