FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Inventory/CommerceHub.Inventory.Api/CommerceHub.Inventory.Api.csproj", "src/Services/Inventory/CommerceHub.Inventory.Api/"]
COPY ["src/Services/Inventory/CommerceHub.Inventory.Application/CommerceHub.Inventory.Application.csproj", "src/Services/Inventory/CommerceHub.Inventory.Application/"]
COPY ["src/Services/Inventory/CommerceHub.Inventory.Infrastructure/CommerceHub.Inventory.Infrastructure.csproj", "src/Services/Inventory/CommerceHub.Inventory.Infrastructure/"]
COPY ["src/Services/Inventory/CommerceHub.Inventory.Domain/CommerceHub.Inventory.Domain.csproj", "src/Services/Inventory/CommerceHub.Inventory.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/Inventory/CommerceHub.Inventory.Api/CommerceHub.Inventory.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Inventory/CommerceHub.Inventory.Api"
RUN dotnet build "CommerceHub.Inventory.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Inventory.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Inventory.Api.dll"]
