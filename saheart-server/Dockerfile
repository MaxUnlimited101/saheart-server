# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET SDK as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY saheart-server.csproj ./
RUN dotnet restore "saheart-server.csproj"

# Copy the rest of the application code and the res folder
COPY . .
WORKDIR "/src"

# Build the application
RUN dotnet build "saheart-server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
RUN dotnet publish "saheart-server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the runtime image to create the final container
FROM base AS final
WORKDIR /app

# Copy the publish output
COPY --from=build /app/publish .

# Copy the res folder
COPY --from=build /src/res ./res

ENTRYPOINT ["dotnet", "saheart-server.dll"]
