FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Vendor/CommerceHub.Vendor.Api/CommerceHub.Vendor.Api.csproj", "src/Services/Vendor/CommerceHub.Vendor.Api/"]
COPY ["src/Services/Vendor/CommerceHub.Vendor.Application/CommerceHub.Vendor.Application.csproj", "src/Services/Vendor/CommerceHub.Vendor.Application/"]
COPY ["src/Services/Vendor/CommerceHub.Vendor.Infrastructure/CommerceHub.Vendor.Infrastructure.csproj", "src/Services/Vendor/CommerceHub.Vendor.Infrastructure/"]
COPY ["src/Services/Vendor/CommerceHub.Vendor.Domain/CommerceHub.Vendor.Domain.csproj", "src/Services/Vendor/CommerceHub.Vendor.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/Vendor/CommerceHub.Vendor.Api/CommerceHub.Vendor.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Vendor/CommerceHub.Vendor.Api"
RUN dotnet build "CommerceHub.Vendor.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Vendor.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Vendor.Api.dll"]
