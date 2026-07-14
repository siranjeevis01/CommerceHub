FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1
ENV MSBuildEnableWorkloadCompiler=false
ENV DOTNET_ROLL_FORWARD=LatestMajor
WORKDIR /src

# Shared libraries
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]

# Domain projects
COPY ["src/Services/Identity/CommerceHub.Identity.Domain/CommerceHub.Identity.Domain.csproj", "src/Services/Identity/CommerceHub.Identity.Domain/"]
COPY ["src/Services/Product/CommerceHub.Product.Domain/CommerceHub.Product.Domain.csproj", "src/Services/Product/CommerceHub.Product.Domain/"]
COPY ["src/Services/Order/CommerceHub.Order.Domain/CommerceHub.Order.Domain.csproj", "src/Services/Order/CommerceHub.Order.Domain/"]
COPY ["src/Services/Cart/CommerceHub.Cart.Domain/CommerceHub.Cart.Domain.csproj", "src/Services/Cart/CommerceHub.Cart.Domain/"]
COPY ["src/Services/Payment/CommerceHub.Payment.Domain/CommerceHub.Payment.Domain.csproj", "src/Services/Payment/CommerceHub.Payment.Domain/"]
COPY ["src/Services/Vendor/CommerceHub.Vendor.Domain/CommerceHub.Vendor.Domain.csproj", "src/Services/Vendor/CommerceHub.Vendor.Domain/"]
COPY ["src/Services/Inventory/CommerceHub.Inventory.Domain/CommerceHub.Inventory.Domain.csproj", "src/Services/Inventory/CommerceHub.Inventory.Domain/"]
COPY ["src/Services/Notification/CommerceHub.Notification.Domain/CommerceHub.Notification.Domain.csproj", "src/Services/Notification/CommerceHub.Notification.Domain/"]
COPY ["src/Services/CMS/CommerceHub.Cms.Domain/CommerceHub.Cms.Domain.csproj", "src/Services/CMS/CommerceHub.Cms.Domain/"]
COPY ["src/Services/Analytics/CommerceHub.Analytics.Domain/CommerceHub.Analytics.Domain.csproj", "src/Services/Analytics/CommerceHub.Analytics.Domain/"]
COPY ["src/Services/AI/CommerceHub.AIAgent.Domain/CommerceHub.AIAgent.Domain.csproj", "src/Services/AI/CommerceHub.AIAgent.Domain/"]

# Application projects
COPY ["src/Services/Identity/CommerceHub.Identity.Application/CommerceHub.Identity.Application.csproj", "src/Services/Identity/CommerceHub.Identity.Application/"]
COPY ["src/Services/Product/CommerceHub.Product.Application/CommerceHub.Product.Application.csproj", "src/Services/Product/CommerceHub.Product.Application/"]
COPY ["src/Services/Order/CommerceHub.Order.Application/CommerceHub.Order.Application.csproj", "src/Services/Order/CommerceHub.Order.Application/"]
COPY ["src/Services/Cart/CommerceHub.Cart.Application/CommerceHub.Cart.Application.csproj", "src/Services/Cart/CommerceHub.Cart.Application/"]
COPY ["src/Services/Payment/CommerceHub.Payment.Application/CommerceHub.Payment.Application.csproj", "src/Services/Payment/CommerceHub.Payment.Application/"]
COPY ["src/Services/Vendor/CommerceHub.Vendor.Application/CommerceHub.Vendor.Application.csproj", "src/Services/Vendor/CommerceHub.Vendor.Application/"]
COPY ["src/Services/Inventory/CommerceHub.Inventory.Application/CommerceHub.Inventory.Application.csproj", "src/Services/Inventory/CommerceHub.Inventory.Application/"]
COPY ["src/Services/Notification/CommerceHub.Notification.Application/CommerceHub.Notification.Application.csproj", "src/Services/Notification/CommerceHub.Notification.Application/"]
COPY ["src/Services/CMS/CommerceHub.Cms.Application/CommerceHub.Cms.Application.csproj", "src/Services/CMS/CommerceHub.Cms.Application/"]
COPY ["src/Services/Analytics/CommerceHub.Analytics.Application/CommerceHub.Analytics.Application.csproj", "src/Services/Analytics/CommerceHub.Analytics.Application/"]
COPY ["src/Services/AI/CommerceHub.AIAgent.Application/CommerceHub.AIAgent.Application.csproj", "src/Services/AI/CommerceHub.AIAgent.Application/"]

# Infrastructure projects
COPY ["src/Services/Identity/CommerceHub.Identity.Infrastructure/CommerceHub.Identity.Infrastructure.csproj", "src/Services/Identity/CommerceHub.Identity.Infrastructure/"]
COPY ["src/Services/Product/CommerceHub.Product.Infrastructure/CommerceHub.Product.Infrastructure.csproj", "src/Services/Product/CommerceHub.Product.Infrastructure/"]
COPY ["src/Services/Order/CommerceHub.Order.Infrastructure/CommerceHub.Order.Infrastructure.csproj", "src/Services/Order/CommerceHub.Order.Infrastructure/"]
COPY ["src/Services/Cart/CommerceHub.Cart.Infrastructure/CommerceHub.Cart.Infrastructure.csproj", "src/Services/Cart/CommerceHub.Cart.Infrastructure/"]
COPY ["src/Services/Payment/CommerceHub.Payment.Infrastructure/CommerceHub.Payment.Infrastructure.csproj", "src/Services/Payment/CommerceHub.Payment.Infrastructure/"]
COPY ["src/Services/Vendor/CommerceHub.Vendor.Infrastructure/CommerceHub.Vendor.Infrastructure.csproj", "src/Services/Vendor/CommerceHub.Vendor.Infrastructure/"]
COPY ["src/Services/Inventory/CommerceHub.Inventory.Infrastructure/CommerceHub.Inventory.Infrastructure.csproj", "src/Services/Inventory/CommerceHub.Inventory.Infrastructure/"]
COPY ["src/Services/Notification/CommerceHub.Notification.Infrastructure/CommerceHub.Notification.Infrastructure.csproj", "src/Services/Notification/CommerceHub.Notification.Infrastructure/"]
COPY ["src/Services/CMS/CommerceHub.Cms.Infrastructure/CommerceHub.Cms.Infrastructure.csproj", "src/Services/CMS/CommerceHub.Cms.Infrastructure/"]
COPY ["src/Services/Analytics/CommerceHub.Analytics.Infrastructure/CommerceHub.Analytics.Infrastructure.csproj", "src/Services/Analytics/CommerceHub.Analytics.Infrastructure/"]
COPY ["src/Services/AI/CommerceHub.AIAgent.Infrastructure/CommerceHub.AIAgent.Infrastructure.csproj", "src/Services/AI/CommerceHub.AIAgent.Infrastructure/"]

# API projects
COPY ["src/ApiGateway/CommerceHub.Gateway/CommerceHub.Gateway.csproj", "src/ApiGateway/CommerceHub.Gateway/"]
COPY ["src/Services/Identity/CommerceHub.Identity.Api/CommerceHub.Identity.Api.csproj", "src/Services/Identity/CommerceHub.Identity.Api/"]
COPY ["src/Services/Product/CommerceHub.Product.Api/CommerceHub.Product.Api.csproj", "src/Services/Product/CommerceHub.Product.Api/"]
COPY ["src/Services/Order/CommerceHub.Order.Api/CommerceHub.Order.Api.csproj", "src/Services/Order/CommerceHub.Order.Api/"]
COPY ["src/Services/Cart/CommerceHub.Cart.Api/CommerceHub.Cart.Api.csproj", "src/Services/Cart/CommerceHub.Cart.Api/"]
COPY ["src/Services/Payment/CommerceHub.Payment.Api/CommerceHub.Payment.Api.csproj", "src/Services/Payment/CommerceHub.Payment.Api/"]
COPY ["src/Services/Vendor/CommerceHub.Vendor.Api/CommerceHub.Vendor.Api.csproj", "src/Services/Vendor/CommerceHub.Vendor.Api/"]
COPY ["src/Services/Inventory/CommerceHub.Inventory.Api/CommerceHub.Inventory.Api.csproj", "src/Services/Inventory/CommerceHub.Inventory.Api/"]
COPY ["src/Services/Notification/CommerceHub.Notification.Api/CommerceHub.Notification.Api.csproj", "src/Services/Notification/CommerceHub.Notification.Api/"]
COPY ["src/Services/CMS/CommerceHub.Cms.Api/CommerceHub.Cms.Api.csproj", "src/Services/CMS/CommerceHub.Cms.Api/"]
COPY ["src/Services/Analytics/CommerceHub.Analytics.Api/CommerceHub.Analytics.Api.csproj", "src/Services/Analytics/CommerceHub.Analytics.Api/"]
COPY ["src/Services/AI/CommerceHub.AIAgent.Api/CommerceHub.AIAgent.Api.csproj", "src/Services/AI/CommerceHub.AIAgent.Api/"]

# Consolidated project
COPY ["src/CommerceHub.Api/CommerceHub.Api.csproj", "src/CommerceHub.Api/"]

RUN dotnet restore "src/CommerceHub.Api/CommerceHub.Api.csproj"

COPY . .
WORKDIR "/src/src/CommerceHub.Api"
RUN dotnet publish "CommerceHub.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:PublishTrimmed=false /p:DebugType=None /p:DebugSymbols=false --no-restore

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV DOTNET_gcServer=0
ENV DOTNET_GCHeapHardLimit=384000000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Api.dll"]
