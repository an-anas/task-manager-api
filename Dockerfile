# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the entire TaskManager.API folder into the image
COPY TaskManager.API/ ./

# Restore dependencies
RUN dotnet restore TaskManager.API.sln

# Publish the application
RUN dotnet publish TaskManager.API/TaskManager.API.csproj -c Release -o out --no-restore

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port your application will run on
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
