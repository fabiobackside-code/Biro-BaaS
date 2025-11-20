# Use the official .NET 8 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY Biro.BaaS.v3/src/Gateway/Biro.API.Gateway/Biro.API.Gateway.csproj ./src/Gateway/Biro.API.Gateway/
RUN dotnet restore ./src/Gateway/Biro.API.Gateway/Biro.API.Gateway.csproj

# Copy the rest of the project files and build the application
COPY Biro.BaaS.v3/src/Gateway/Biro.API.Gateway/. ./src/Gateway/Biro.API.Gateway/
RUN dotnet publish -c Release -o out ./src/Gateway/Biro.API.Gateway/Biro.API.Gateway.csproj

# Use the official ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Biro.API.Gateway.dll"]
