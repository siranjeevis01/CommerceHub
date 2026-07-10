FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Notification/CommerceHub.Notification.Api/CommerceHub.Notification.Api.csproj", "src/Services/Notification/CommerceHub.Notification.Api/"]
COPY ["src/Services/Notification/CommerceHub.Notification.Application/CommerceHub.Notification.Application.csproj", "src/Services/Notification/CommerceHub.Notification.Application/"]
COPY ["src/Services/Notification/CommerceHub.Notification.Domain/CommerceHub.Notification.Domain.csproj", "src/Services/Notification/CommerceHub.Notification.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
RUN dotnet restore "src/Services/Notification/CommerceHub.Notification.Api/CommerceHub.Notification.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Notification/CommerceHub.Notification.Api"
RUN dotnet build "CommerceHub.Notification.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Notification.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Notification.Api.dll"]
