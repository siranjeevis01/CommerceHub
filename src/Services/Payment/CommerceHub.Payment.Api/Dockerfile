FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Payment/CommerceHub.Payment.Api/CommerceHub.Payment.Api.csproj", "src/Services/Payment/CommerceHub.Payment.Api/"]
COPY ["src/Services/Payment/CommerceHub.Payment.Application/CommerceHub.Payment.Application.csproj", "src/Services/Payment/CommerceHub.Payment.Application/"]
COPY ["src/Services/Payment/CommerceHub.Payment.Infrastructure/CommerceHub.Payment.Infrastructure.csproj", "src/Services/Payment/CommerceHub.Payment.Infrastructure/"]
COPY ["src/Services/Payment/CommerceHub.Payment.Domain/CommerceHub.Payment.Domain.csproj", "src/Services/Payment/CommerceHub.Payment.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/Payment/CommerceHub.Payment.Api/CommerceHub.Payment.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Payment/CommerceHub.Payment.Api"
RUN dotnet build "CommerceHub.Payment.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Payment.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Payment.Api.dll"]
