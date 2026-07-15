FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Shared infrastructure projects
COPY ["backend/src/SharedKernel/CommerceHub.SharedKernel.csproj", "backend/src/SharedKernel/"]
COPY ["backend/src/Infrastructure/CommerceHub.Infrastructure.csproj", "backend/src/Infrastructure/"]
COPY ["backend/src/ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "backend/src/ServiceDefaults/"]
COPY ["backend/src/BuildingBlocks/CommerceHub.BuildingBlocks.csproj", "backend/src/BuildingBlocks/"]
COPY ["backend/src/BuildingBlocks/Contracts/CommerceHub.Shared.Contracts.csproj", "backend/src/BuildingBlocks/Contracts/"]
COPY ["backend/src/BuildingBlocks/Messaging/CommerceHub.Shared.Messaging.csproj", "backend/src/BuildingBlocks/Messaging/"]

# Api project
COPY ["backend/src/Api/CommerceHub.Api.csproj", "backend/src/Api/"]

# Identity module
COPY ["backend/src/Modules/Identity/Domain/CommerceHub.Modules.Identity.Domain.csproj", "backend/src/Modules/Identity/Domain/"]
COPY ["backend/src/Modules/Identity/Application/CommerceHub.Modules.Identity.Application.csproj", "backend/src/Modules/Identity/Application/"]
COPY ["backend/src/Modules/Identity/Infrastructure/CommerceHub.Modules.Identity.Infrastructure.csproj", "backend/src/Modules/Identity/Infrastructure/"]

# Product module
COPY ["backend/src/Modules/Product/Domain/CommerceHub.Modules.Product.Domain.csproj", "backend/src/Modules/Product/Domain/"]
COPY ["backend/src/Modules/Product/Application/CommerceHub.Modules.Product.Application.csproj", "backend/src/Modules/Product/Application/"]
COPY ["backend/src/Modules/Product/Infrastructure/CommerceHub.Modules.Product.Infrastructure.csproj", "backend/src/Modules/Product/Infrastructure/"]

# Order module
COPY ["backend/src/Modules/Order/Domain/CommerceHub.Modules.Order.Domain.csproj", "backend/src/Modules/Order/Domain/"]
COPY ["backend/src/Modules/Order/Application/CommerceHub.Modules.Order.Application.csproj", "backend/src/Modules/Order/Application/"]
COPY ["backend/src/Modules/Order/Infrastructure/CommerceHub.Modules.Order.Infrastructure.csproj", "backend/src/Modules/Order/Infrastructure/"]

# Cart module
COPY ["backend/src/Modules/Cart/Domain/CommerceHub.Modules.Cart.Domain.csproj", "backend/src/Modules/Cart/Domain/"]
COPY ["backend/src/Modules/Cart/Application/CommerceHub.Modules.Cart.Application.csproj", "backend/src/Modules/Cart/Application/"]
COPY ["backend/src/Modules/Cart/Infrastructure/CommerceHub.Modules.Cart.Infrastructure.csproj", "backend/src/Modules/Cart/Infrastructure/"]

# Payment module
COPY ["backend/src/Modules/Payment/Domain/CommerceHub.Modules.Payment.Domain.csproj", "backend/src/Modules/Payment/Domain/"]
COPY ["backend/src/Modules/Payment/Application/CommerceHub.Modules.Payment.Application.csproj", "backend/src/Modules/Payment/Application/"]
COPY ["backend/src/Modules/Payment/Infrastructure/CommerceHub.Modules.Payment.Infrastructure.csproj", "backend/src/Modules/Payment/Infrastructure/"]

# Vendor module
COPY ["backend/src/Modules/Vendor/Domain/CommerceHub.Modules.Vendor.Domain.csproj", "backend/src/Modules/Vendor/Domain/"]
COPY ["backend/src/Modules/Vendor/Application/CommerceHub.Modules.Vendor.Application.csproj", "backend/src/Modules/Vendor/Application/"]
COPY ["backend/src/Modules/Vendor/Infrastructure/CommerceHub.Modules.Vendor.Infrastructure.csproj", "backend/src/Modules/Vendor/Infrastructure/"]

# Inventory module
COPY ["backend/src/Modules/Inventory/Domain/CommerceHub.Modules.Inventory.Domain.csproj", "backend/src/Modules/Inventory/Domain/"]
COPY ["backend/src/Modules/Inventory/Application/CommerceHub.Modules.Inventory.Application.csproj", "backend/src/Modules/Inventory/Application/"]
COPY ["backend/src/Modules/Inventory/Infrastructure/CommerceHub.Modules.Inventory.Infrastructure.csproj", "backend/src/Modules/Inventory/Infrastructure/"]

# Notification module
COPY ["backend/src/Modules/Notification/Domain/CommerceHub.Modules.Notification.Domain.csproj", "backend/src/Modules/Notification/Domain/"]
COPY ["backend/src/Modules/Notification/Application/CommerceHub.Modules.Notification.Application.csproj", "backend/src/Modules/Notification/Application/"]
COPY ["backend/src/Modules/Notification/Infrastructure/CommerceHub.Modules.Notification.Infrastructure.csproj", "backend/src/Modules/Notification/Infrastructure/"]

# Cms module
COPY ["backend/src/Modules/Cms/Domain/CommerceHub.Modules.Cms.Domain.csproj", "backend/src/Modules/Cms/Domain/"]
COPY ["backend/src/Modules/Cms/Application/CommerceHub.Modules.Cms.Application.csproj", "backend/src/Modules/Cms/Application/"]
COPY ["backend/src/Modules/Cms/Infrastructure/CommerceHub.Modules.Cms.Infrastructure.csproj", "backend/src/Modules/Cms/Infrastructure/"]

# Analytics module
COPY ["backend/src/Modules/Analytics/Domain/CommerceHub.Modules.Analytics.Domain.csproj", "backend/src/Modules/Analytics/Domain/"]
COPY ["backend/src/Modules/Analytics/Application/CommerceHub.Modules.Analytics.Application.csproj", "backend/src/Modules/Analytics/Application/"]
COPY ["backend/src/Modules/Analytics/Infrastructure/CommerceHub.Modules.Analytics.Infrastructure.csproj", "backend/src/Modules/Analytics/Infrastructure/"]

# Ai module
COPY ["backend/src/Modules/Ai/Domain/CommerceHub.Modules.Ai.Domain.csproj", "backend/src/Modules/Ai/Domain/"]
COPY ["backend/src/Modules/Ai/Application/CommerceHub.Modules.Ai.Application.csproj", "backend/src/Modules/Ai/Application/"]
COPY ["backend/src/Modules/Ai/Infrastructure/CommerceHub.Modules.Ai.Infrastructure.csproj", "backend/src/Modules/Ai/Infrastructure/"]

RUN dotnet restore "backend/src/Api/CommerceHub.Api.csproj"

COPY backend backend/
WORKDIR "/src/backend/src/Api"
RUN dotnet publish "CommerceHub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Api.dll"]
