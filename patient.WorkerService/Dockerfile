FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["patient.WorkerService/patient.WorkerService.csproj", "patient.WorkerService/"]
COPY ["patient.infrastructure/patient.infrastructure.csproj", "patient.infrastructure/"]
COPY ["patient.application/patient.application.csproj", "patient.application/"]
COPY ["patient.domain/patient.domain.csproj", "patient.domain/"]
RUN dotnet restore "patient.WorkerService/patient.WorkerService.csproj"
COPY . .
WORKDIR "/src/patient.WorkerService"
RUN dotnet build "patient.WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "patient.WorkerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "patient.WorkerService.dll"]
