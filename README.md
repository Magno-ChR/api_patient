## Diagrama de clases

![Diagrama de clases](Docs/diagrama-clases.png)

## Linting y pre-commit

- **.editorconfig**: Configuración de estilo (tabs, no espacios; reglas C#). Aplicada por el editor y por `dotnet format`.
- **SonarLint**: Se recomienda instalar la extensión [SonarLint](https://marketplace.visualstudio.com/items?itemName=SonarSource.sonarlint-vscode) en VS Code (o SonarLint para Visual Studio). El proyecto sugiere la extensión en `.vscode/extensions.json`.
- **Husky + pre-commit**: Antes de cada commit se ejecuta:
  - **lint-staged**: aplica `dotnet format` a los `.cs` en stage según `.editorconfig` (incluye la regla de tabs).
  - **commit-msg** (commitlint): valida que el mensaje siga Conventional Commits (`feat:`, `fix:`, `chore:`, etc.).

### Configuración inicial

```bash
npm install
```

Con `npm install` se instalan Husky, lint-staged y commitlint. El script `prepare` configura los hooks de Husky automáticamente.

## Observabilidad - Fase 1

- Levantar el entorno local (API, worker, base de datos y monitoreo):

```bash
docker compose -f docker-compose-dev.yml up -d
```

- Health checks API:
  - `GET /health/live` (liveness)
  - `GET /health/ready` (readiness con prueba de conexion a PostgreSQL)
- Metricas Prometheus API:
  - `GET /metrics`
- Stack de monitoreo en `docker-compose-dev.yml`:
  - Prometheus: `http://localhost:9090`
  - Grafana: `http://localhost:3000` (usuario `admin`, password `admin`)
  - Node Exporter: `http://localhost:9100/metrics`

## Observabilidad - Fase 2

- Trazas distribuidas con OpenTelemetry (API + Worker + propagacion de contexto en RabbitMQ).
- Logs centralizados con Loki + Promtail (captura de logs de contenedores Docker).
- Visualizacion unificada en Grafana con data sources provisionados:
  - Prometheus
  - Loki
  - Tempo
- Endpoints relevantes:
  - Tempo: `http://localhost:3200`
  - Loki: `http://localhost:3100`
  - OTel Collector (OTLP gRPC): `http://localhost:4317`

## Observabilidad - Fase 3 (inicio)

- Serilog integrado en API y Worker como proveedor de `ILogger`.
- Logs estructurados en formato JSON por consola (capturados por Promtail/Loki).
- Enriquecimiento con contexto de traza (`TraceId` y `SpanId`) para correlacionar logs con Tempo.