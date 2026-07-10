using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["JwtKey"]
    ?? throw new InvalidOperationException(
        "The 'JwtKey' configuration value is not set. " +
        "Set it via dotnet user-secrets, environment variable, or appsettings.json.");

var mysql = builder.AddMySql("mysql")
    .WithEnvironment("MYSQL_ROOT_PASSWORD", builder.Configuration["DbRootPassword"] ?? "rootpassword");

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var elasticsearch = builder.AddElasticsearch("elasticsearch")
    .WithHealthCheck("elasticsearch");

var postgres = builder.AddPostgres("postgres")
    .WithHealthCheck("postgres");

var seq = builder.AddSeq("seq");

var projectDir = Path.GetFullPath(Path.Combine("..", "..", "Services"));

var identityDb = mysql.AddDatabase("commercehub-identity");
var identityService = builder.AddProject("identity-service", Path.Combine(projectDir, "Identity", "CommerceHub.Identity.Api", "CommerceHub.Identity.Api.csproj"))
    .WithReference(identityDb)
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WithEnvironment("JWT_KEY", jwtKey)
    .WithEnvironment("JWT_ISSUER", "CommerceHub")
    .WithEnvironment("JWT_AUDIENCE", "CommerceHubClient")
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var productDb = mysql.AddDatabase("commercehub-products");
var productService = builder.AddProject("product-service", Path.Combine(projectDir, "Product", "CommerceHub.Product.Api", "CommerceHub.Product.Api.csproj"))
    .WithReference(productDb)
    .WithReference(rabbitmq)
    .WithReference(elasticsearch)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var orderDb = mysql.AddDatabase("commercehub-orders");
var orderService = builder.AddProject("order-service", Path.Combine(projectDir, "Order", "CommerceHub.Order.Api", "CommerceHub.Order.Api.csproj"))
    .WithReference(orderDb)
    .WithReference(rabbitmq)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var cartService = builder.AddProject("cart-service", Path.Combine(projectDir, "Cart", "CommerceHub.Cart.Api", "CommerceHub.Cart.Api.csproj"))
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var paymentDb = mysql.AddDatabase("commercehub-payments");
var paymentService = builder.AddProject("payment-service", Path.Combine(projectDir, "Payment", "CommerceHub.Payment.Api", "CommerceHub.Payment.Api.csproj"))
    .WithReference(paymentDb)
    .WithReference(rabbitmq)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var vendorDb = mysql.AddDatabase("commercehub-vendors");
var vendorService = builder.AddProject("vendor-service", Path.Combine(projectDir, "Vendor", "CommerceHub.Vendor.Api", "CommerceHub.Vendor.Api.csproj"))
    .WithReference(vendorDb)
    .WithReference(rabbitmq)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var inventoryDb = mysql.AddDatabase("commercehub-inventory");
var inventoryService = builder.AddProject("inventory-service", Path.Combine(projectDir, "Inventory", "CommerceHub.Inventory.Api", "CommerceHub.Inventory.Api.csproj"))
    .WithReference(inventoryDb)
    .WithReference(rabbitmq)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var notificationService = builder.AddProject("notification-service", Path.Combine(projectDir, "Notification", "CommerceHub.Notification.Api", "CommerceHub.Notification.Api.csproj"))
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var cmsDb = mysql.AddDatabase("commercehub-cms");
var cmsService = builder.AddProject("cms-service", Path.Combine(projectDir, "CMS", "CommerceHub.Cms.Api", "CommerceHub.Cms.Api.csproj"))
    .WithReference(cmsDb)
    .WithReference(redis)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var analyticsDb = postgres.AddDatabase("commercehub-analytics");
var analyticsService = builder.AddProject("analytics-service", Path.Combine(projectDir, "Analytics", "CommerceHub.Analytics.Api", "CommerceHub.Analytics.Api.csproj"))
    .WithReference(analyticsDb)
    .WithReference(rabbitmq)
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

var gateway = builder.AddProject("gateway", Path.Combine("..", "..", "ApiGateway", "CommerceHub.Gateway", "CommerceHub.Gateway.csproj"))
    .WithReference(identityService)
    .WithReference(productService)
    .WithReference(orderService)
    .WithReference(cartService)
    .WithReference(paymentService)
    .WithReference(vendorService)
    .WithReference(inventoryService)
    .WithReference(notificationService)
    .WithReference(cmsService)
    .WithReference(analyticsService)
    .WithEnvironment("JWT_KEY", jwtKey)
    .WithEnvironment("JWT_ISSUER", "CommerceHub")
    .WithEnvironment("JWT_AUDIENCE", "CommerceHubClient")
    .WithEnvironment("ALLOWED_ORIGINS", builder.Configuration["AllowedOrigins"] ?? "http://localhost:3000")
    .WithEnvironment("OTLP_EXPORTER_ENDPOINT", builder.Configuration["OtlpExporterEndpoint"] ?? "http://localhost:4317")
    .WithEnvironment("SEQ_URL", seq.Resource.ConnectionStringExpression)
    .WithExternalHttpEndpoints();

builder.Build().Run();
