# 1ª Fase: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Core/*.csproj ./Core/
COPY Application/*.csproj ./Application/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY AgroSolutions.Users/*.csproj ./AgroSolutions.Users/

RUN dotnet restore ./AgroSolutions.Users/AgroSolutions.Users.csproj

COPY . .

RUN dotnet publish ./AgroSolutions.Users/AgroSolutions.Users.csproj -c Release -o /app --no-restore

# 2ª Fase: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

RUN addgroup -g 1000 appgroup && adduser -u 1000 -G appgroup -s /bin/sh -D appuser
RUN chown -R appuser:appgroup /app
USER appuser

COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgroSolutions.Users.dll"]
