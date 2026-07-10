FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Analytics/CommerceHub.Analytics.Api/CommerceHub.Analytics.Api.csproj", "src/Services/Analytics/CommerceHub.Analytics.Api/"]
COPY ["src/Services/Analytics/CommerceHub.Analytics.Application/CommerceHub.Analytics.Application.csproj", "src/Services/Analytics/CommerceHub.Analytics.Application/"]
COPY ["src/Services/Analytics/CommerceHub.Analytics.Infrastructure/CommerceHub.Analytics.Infrastructure.csproj", "src/Services/Analytics/CommerceHub.Analytics.Infrastructure/"]
COPY ["src/Services/Analytics/CommerceHub.Analytics.Domain/CommerceHub.Analytics.Domain.csproj", "src/Services/Analytics/CommerceHub.Analytics.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/Analytics/CommerceHub.Analytics.Api/CommerceHub.Analytics.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Analytics/CommerceHub.Analytics.Api"
RUN dotnet build "CommerceHub.Analytics.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Analytics.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Analytics.Api.dll"]
