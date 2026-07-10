FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/CMS/CommerceHub.Cms.Api/CommerceHub.Cms.Api.csproj", "src/Services/CMS/CommerceHub.Cms.Api/"]
COPY ["src/Services/CMS/CommerceHub.Cms.Application/CommerceHub.Cms.Application.csproj", "src/Services/CMS/CommerceHub.Cms.Application/"]
COPY ["src/Services/CMS/CommerceHub.Cms.Infrastructure/CommerceHub.Cms.Infrastructure.csproj", "src/Services/CMS/CommerceHub.Cms.Infrastructure/"]
COPY ["src/Services/CMS/CommerceHub.Cms.Domain/CommerceHub.Cms.Domain.csproj", "src/Services/CMS/CommerceHub.Cms.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/CMS/CommerceHub.Cms.Api/CommerceHub.Cms.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/CMS/CommerceHub.Cms.Api"
RUN dotnet build "CommerceHub.Cms.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Cms.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Cms.Api.dll"]
