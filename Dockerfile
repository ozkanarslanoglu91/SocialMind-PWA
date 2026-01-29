# SocialMind Web
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SocialMind.sln", "./"]
COPY ["SocialMind/SocialMind.Web/SocialMind.Web.csproj", "SocialMind/SocialMind.Web/"]
COPY ["SocialMind/SocialMind.Shared/SocialMind.Shared.csproj", "SocialMind/SocialMind.Shared/"]

# Restore packages
RUN dotnet restore "SocialMind/SocialMind.Web/SocialMind.Web.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/SocialMind/SocialMind.Web"
RUN dotnet build "SocialMind.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SocialMind.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SocialMind.Web.dll"]
