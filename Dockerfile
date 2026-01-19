# -----------------------------
# Stage 1: Base runtime
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Render assigns a port via environment variable
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Ensure Data Protection folder exists
RUN mkdir -p /tmp/keys

# -----------------------------
# Stage 2: Build
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["MoneyTrackrView/MoneyTrackrView.csproj", "MoneyTrackrView/"]
COPY ["MoneyTrackr.Borrowers/MoneyTrackr.Borrowers.csproj", "MoneyTrackr.Borrowers/"]

RUN dotnet restore "./MoneyTrackrView/MoneyTrackrView.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/MoneyTrackrView"

# Publish the app
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MoneyTrackrView.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------------
# Stage 3: Final image
# -----------------------------
FROM base AS final
WORKDIR /app

# Copy published app from build stage
COPY --from=build /app/publish .

# Ensure keys folder exists at runtime
RUN mkdir -p /tmp/keys

# Start the app
ENTRYPOINT ["dotnet", "MoneyTrackrView.dll"]
