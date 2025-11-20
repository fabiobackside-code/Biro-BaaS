# Use the official .NET 8 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY Biro.BaaS.v3/src/Blocks/Biro.Blocks.Credit/Biro.Blocks.Credit.csproj ./src/Blocks/Biro.Blocks.Credit/
COPY Biro.BaaS.v3/Biro-BaaS-repo/bks-sdk/bks-sdk.csproj ./Biro-BaaS-repo/bks-sdk/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Persistence.Dapper/Biro.Infrastructure.Persistence.Dapper.csproj ./src/Infrastructure/Biro.Infrastructure.Persistence.Dapper/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Messaging/Biro.Infrastructure.Messaging.csproj ./src/Infrastructure/Biro.Infrastructure.Messaging/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Observability/Biro.Infrastructure.Observability.csproj ./src/Infrastructure/Biro.Infrastructure.Observability/
COPY Biro.BaaS.v3/src/Core/Biro.Core.Application/Biro.Core.Application.csproj ./src/Core/Biro.Core.Application/
COPY Biro.BaaS.v3/src/Core/Biro.Core.Domain/Biro.Core.Domain.csproj ./src/Core/Biro.Core.Domain/
RUN dotnet restore ./src/Blocks/Biro.Blocks.Credit/Biro.Blocks.Credit.csproj

# Copy the rest of the project files and build the application
COPY Biro.BaaS.v3/src/Blocks/Biro.Blocks.Credit/. ./src/Blocks/Biro.Blocks.Credit/
COPY Biro.BaaS.v3/Biro-BaaS-repo/bks-sdk/. ./Biro-BaaS-repo/bks-sdk/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Persistence.Dapper/. ./src/Infrastructure/Biro.Infrastructure.Persistence.Dapper/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Messaging/. ./src/Infrastructure/Biro.Infrastructure.Messaging/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Observability/. ./src/Infrastructure/Biro.Infrastructure.Observability/
COPY Biro.BaaS.v3/src/Core/Biro.Core.Application/. ./src/Core/Biro.Core.Application/
COPY Biro.BaaS.v3/src/Core/Biro.Core.Domain/. ./src/Core/Biro.Core.Domain/
RUN dotnet publish -c Release -o out ./src/Blocks/Biro.Blocks.Credit/Biro.Blocks.Credit.csproj

# Use the official ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Biro.Blocks.Credit.dll"]
