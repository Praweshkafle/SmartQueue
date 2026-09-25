
# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SmartQueue.API/SmartQueue.API.csproj", "SmartQueue.API/"]
COPY ["SmartQueue.Application/SmartQueue.Application.csproj", "SmartQueue.Application/"]
COPY ["SmartQueue.Infrastructure/SmartQueue.Infrastructure.csproj", "SmartQueue.Infrastructure/"]
COPY ["SmartQueue.Domain/SmartQueue.Domain.csproj", "SmartQueue.Domain/"]

RUN dotnet restore "SmartQueue.API/SmartQueue.API.csproj"

COPY . .

WORKDIR "/src/SmartQueue.API"
RUN dotnet publish "SmartQueue.API.csproj" -c Release -o /app/publish

# Stage 2 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SmartQueue.API.dll"]