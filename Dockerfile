# Etapa base para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar todos los archivos .csproj de los proyectos
COPY ["api_patient/api_patient.csproj", "api_patient/"]
COPY ["patient.application/patient.application.csproj", "patient.application/"]
COPY ["patient.domain/patient.domain.csproj", "patient.domain/"]
COPY ["patient.infrastructure/patient.infrastructure.csproj", "patient.infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "api_patient/api_patient.csproj"

# Copiar todo el código
COPY . .

# Compilar el proyecto principal
WORKDIR "/src/api_patient"
RUN dotnet build "api_patient.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar para despliegue
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api_patient.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final (runtime)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api_patient.dll"]
