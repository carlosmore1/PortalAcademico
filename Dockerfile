# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# Ruta para SQLite (Render montará el disco en /var/data si lo configuras)
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
COPY --from=build /app/out ./
# La app leerá ConnectionStrings__DefaultConnection y Redis__ConnectionString de las env vars
ENTRYPOINT ["dotnet", "PortalAcademico.dll"]
