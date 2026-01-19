FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MoneyTrackrView/MoneyTrackrView.csproj", "MoneyTrackrView/"]
COPY ["MoneyTrackr.Borrowers/MoneyTrackr.Borrowers.csproj", "MoneyTrackr.Borrowers/"]
RUN dotnet restore "./MoneyTrackrView/MoneyTrackrView.csproj"
COPY . .
WORKDIR "/src/MoneyTrackrView"
RUN dotnet publish "MoneyTrackrView.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MoneyTrackrView.dll"]
