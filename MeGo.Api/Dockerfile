# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["MeGo.Api.csproj", "./"]
RUN dotnet restore "MeGo.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "MeGo.Api.csproj" -c Release -o /app/publish

# Use the runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port (Render will set PORT env var)
EXPOSE 10000

# Run the app
ENTRYPOINT ["dotnet", "MeGo.Api.dll"]



