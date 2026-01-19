# =========================
# Stage 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore
COPY ["MoneyTrackrView/MoneyTrackrView.csproj", "MoneyTrackrView/"]
COPY ["MoneyTrackr.Borrowers/MoneyTrackr.Borrowers.csproj", "MoneyTrackr.Borrowers/"]

# Restore dependencies
RUN dotnet restore "./MoneyTrackrView/MoneyTrackrView.csproj"

# Copy all source files
COPY . .
WORKDIR "/src/MoneyTrackrView"

# Build the project
RUN dotnet build "./MoneyTrackrView.csproj" -c $BUILD_CONFIGURATION -o /app/build

# =========================
# Stage 2: Publish
# =========================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publish the application
RUN dotnet publish "./MoneyTrackrView.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# =========================
# Stage 3: Final Runtime Image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Prepare keys volume
RUN mkdir -p /app/keys
VOLUME ["/app/keys"]

# Copy published app from previous stage
COPY --from=publish /app/publish .

# Entry point
ENTRYPOINT ["dotnet", "MoneyTrackrView.dll"]
