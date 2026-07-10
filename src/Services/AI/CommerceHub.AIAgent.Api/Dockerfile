FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/AI/CommerceHub.AIAgent.Api/CommerceHub.AIAgent.Api.csproj", "src/Services/AI/CommerceHub.AIAgent.Api/"]
COPY ["src/Services/AI/CommerceHub.AIAgent.Application/CommerceHub.AIAgent.Application.csproj", "src/Services/AI/CommerceHub.AIAgent.Application/"]
COPY ["src/Services/AI/CommerceHub.AIAgent.Domain/CommerceHub.AIAgent.Domain.csproj", "src/Services/AI/CommerceHub.AIAgent.Domain/"]
COPY ["src/Services/AI/CommerceHub.AIAgent.Infrastructure/CommerceHub.AIAgent.Infrastructure.csproj", "src/Services/AI/CommerceHub.AIAgent.Infrastructure/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/AI/CommerceHub.AIAgent.Api/CommerceHub.AIAgent.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/AI/CommerceHub.AIAgent.Api"
RUN dotnet build "CommerceHub.AIAgent.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.AIAgent.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.AIAgent.Api.dll"]