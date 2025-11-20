# Use the official .NET 8 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY Biro.BaaS.v3/src/Blocks/Biro.Blocks.Authentication/Biro.Blocks.Authentication.csproj ./src/Blocks/Biro.Blocks.Authentication/
COPY Biro.BaaS.v3/Biro-BaaS-repo/bks-sdk/bks-sdk.csproj ./Biro-BaaS-repo/bks-sdk/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Observability/Biro.Infrastructure.Observability.csproj ./src/Infrastructure/Biro.Infrastructure.Observability/
RUN dotnet restore ./src/Blocks/Biro.Blocks.Authentication/Biro.Blocks.Authentication.csproj

# Copy the rest of the project files and build the application
COPY Biro.BaaS.v3/src/Blocks/Biro.Blocks.Authentication/. ./src/Blocks/Biro.Blocks.Authentication/
COPY Biro.BaaS.v3/Biro-BaaS-repo/bks-sdk/. ./Biro-BaaS-repo/bks-sdk/
COPY Biro.BaaS.v3/src/Infrastructure/Biro.Infrastructure.Observability/. ./src/Infrastructure/Biro.Infrastructure.Observability/
RUN dotnet publish -c Release -o out ./src/Blocks/Biro.Blocks.Authentication/Biro.Blocks.Authentication.csproj

# Use the official ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Biro.Blocks.Authentication.dll"]
