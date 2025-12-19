FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IEEEBackend.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-80}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "IEEEBackend.dll"]
