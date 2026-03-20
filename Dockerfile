# Dockerfile para sa ASP.NET Core
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj at restore
COPY *.csproj .
RUN dotnet restore

# Copy lahat at build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
EXPOSE 443
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RFIDAttendanceAPI.dll"]