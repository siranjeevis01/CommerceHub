FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/Product/CommerceHub.Product.Api/CommerceHub.Product.Api.csproj", "src/Services/Product/CommerceHub.Product.Api/"]
COPY ["src/Services/Product/CommerceHub.Product.Application/CommerceHub.Product.Application.csproj", "src/Services/Product/CommerceHub.Product.Application/"]
COPY ["src/Services/Product/CommerceHub.Product.Infrastructure/CommerceHub.Product.Infrastructure.csproj", "src/Services/Product/CommerceHub.Product.Infrastructure/"]
COPY ["src/Services/Product/CommerceHub.Product.Domain/CommerceHub.Product.Domain.csproj", "src/Services/Product/CommerceHub.Product.Domain/"]
COPY ["src/Shared/CommerceHub.Shared.Kernel/CommerceHub.Shared.Kernel.csproj", "src/Shared/CommerceHub.Shared.Kernel/"]
COPY ["src/Shared/CommerceHub.Shared.Messaging/CommerceHub.Shared.Messaging.csproj", "src/Shared/CommerceHub.Shared.Messaging/"]
COPY ["src/Shared/CommerceHub.Shared.Contracts/CommerceHub.Shared.Contracts.csproj", "src/Shared/CommerceHub.Shared.Contracts/"]
COPY ["src/Shared/CommerceHub.ServiceDefaults/CommerceHub.ServiceDefaults.csproj", "src/Shared/CommerceHub.ServiceDefaults/"]
RUN dotnet restore "src/Services/Product/CommerceHub.Product.Api/CommerceHub.Product.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Product/CommerceHub.Product.Api"
RUN dotnet build "CommerceHub.Product.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommerceHub.Product.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommerceHub.Product.Api.dll"]
