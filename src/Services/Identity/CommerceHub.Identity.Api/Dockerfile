FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Identity/CommerceHub.Identity.Api/CommerceHub.Identity.Api.csproj", "src/Services/Identity/CommerceHub.Identity.Api/"]
COPY ["src/Services/Identity/CommerceHub.Identity.Application/CommerceHub.Identity.Application.csproj", "src/Services/Identity/CommerceHub.Identity.Application/"]
COPY ["src/Services/Identity/CommerceHub.Identity.Infrastructure/CommerceHub.Identity.Infrastructure.csproj", "src/Services/Identity/CommerceHub.Identity.Infrastructure/"]
COPY ["src/Services/Identity/CommerceHub.Identity.Domain/CommerceHub.Identity.Domain.csproj", "src/Services/Identity/CommerceHub.Identity.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/Identity/CommerceHub.Identity.Api/CommerceHub.Identity.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Identity/CommerceHub.Identity.Api"
RUN dotnet build "CommerceHub.Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Identity.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Identity.Api.dll"]
