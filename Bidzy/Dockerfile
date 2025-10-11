# Use .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Bidzy.csproj .
RUN dotnet restore

# Copy the rest of the project files
COPY . .

# Build and publish
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Expose port 8080
EXPOSE 8080

# Run your app (note: it's Bidzy.dll)
ENTRYPOINT ["dotnet", "Bidzy.dll"]
