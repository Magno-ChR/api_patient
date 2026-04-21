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

## Observabilidad (Serilog + Loki/Seq, OpenTelemetry, Prometheus, Grafana)

La aplicación (API, worker, PostgreSQL) vive en **`docker-compose-dev.yml`**. El repo también trae un stack local de referencia en **`docker-compose.observability.yml`**, pero no es obligatorio: la app puede conectarse a infraestructura externa de `RabbitMQ`, `Loki`, `Seq` y `OTLP/Jaeger`.

### Arranque local recomendado

La red **`develop_nutricenter_net`** es **externa** en ambos composes. Créala si no existe (en el servidor ya debería existir):

```bash
docker network create develop_nutricenter_net
```

1. Variables de entorno para RabbitMQ y observabilidad:

```bash
cp docker-compose.env.example .env
```

Edita `.env` con los datos reales de tu broker y de tus backends de observabilidad.

2. Aplicación:

```bash
docker compose -f docker-compose-dev.yml up -d --build
```

3. Observabilidad (desde la raíz del repo):

```bash
cp monitoring/observability.env.example monitoring/observability.env
# Edita monitoring/observability.env (usuario/contraseña Grafana, etc.)
docker compose -f docker-compose.observability.yml --env-file monitoring/observability.env up -d
```

### URLs y credenciales del stack local de referencia

| Servicio    | URL local típica     | Usuario / contraseña |
|------------|----------------------|----------------------|
| API        | `http://localhost:5002` | JWT según configuración |
| Grafana    | `http://localhost:3000`   | Con `--env-file monitoring/observability.env`: valores del archivo (ejemplo: `admin` / `CambiarGrafanaEnProduccion`). Sin env-file: `admin` / `admin` |
| Prometheus | `http://localhost:9090`   | Sin autenticación (restringir en producción con proxy o red) |
| Seq        | `http://localhost:5341`   | Primera visita: creas el usuario administrador en la UI de Seq |
| Jaeger UI  | `http://localhost:16686`  | Sin autenticación en `all-in-one` (no exponer a Internet sin protección) |

Con el stack local de referencia, **api-patient** y **patient-worker** pueden enviar trazas OTLP y logs a backends que estén en la misma red Docker. Si `OTEL_EXPORTER_OTLP_ENDPOINT`, `LOKI_URL` o `SEQ_SERVER_URL` quedan vacíos, la aplicación sigue arrancando y mantiene solo el logging de consola configurado en `appsettings`.

En ejecución local con `dotnet run`, puedes exportar definiendo en el sistema `OTEL_EXPORTER_OTLP_ENDPOINT`, `LOKI_URL` y `SEQ_SERVER_URL` solo cuando esos endpoints sean accesibles.

**Nota:** Grafana no recibe datos directamente de la app. Grafana consume datasources ya configurados, típicamente `Prometheus`, `Loki` y `Jaeger` o `Tempo`. Por eso, para una infraestructura externa, además de la URL de Grafana necesitas los endpoints reales a los que la app enviará logs y trazas.

### Health checks y métricas (API)

- `GET /health/live` — liveness
- `GET /health/ready` — readiness (incluye PostgreSQL)
- `GET /metrics` — métricas Prometheus

## Infraestructura externa

Si en tu servidor solo desplegarás este proyecto y **no** levantarás `docker-compose.observability.yml`, la configuración mínima va en `.env` o en variables del entorno/CI.

Variables principales:

```env
INFRA_HOST=165.22.148.216
RABBITMQ__PORT=5672
RABBITMQ__USERNAME=admin
RABBITMQ__PASSWORD=admin
RABBITMQ__VIRTUALHOST=/
RABBITMQ__FOODPLANS_EXCHANGE=meal-plans
RABBITMQ__FOODPLANS_QUEUE=ms-patients-queue
RABBITMQ__FOODPLAN_CREATED_ROUTING_KEY=meal-plan.created
RABBITMQ__FOODPLAN_UPDATED_ROUTING_KEY=meal-plan.updated
RABBITMQ__PATIENTS_EXCHANGE=patients
RABBITMQ__PATIENT_CREATED_ROUTING_KEY=patient.created
RABBITMQ__PATIENT_UPDATED_ROUTING_KEY=patient.updated
OTEL_EXPORTER_OTLP_ENDPOINT=
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
LOKI_URL=
SEQ_SERVER_URL=
```

Con `INFRA_HOST`, `docker-compose-dev.yml` replica el patrón de **ms-logistic**:

- `RabbitMQ__HostName` usa `INFRA_HOST` si existe; si no, cae a `RABBITMQ__HOSTNAME` y por último a `rabbitmq` (AMQP, normalmente puerto `5672`).
- `Telemetry__OtlpEndpoint` queda en `http://<ese host>:4317`.
- Se añade Serilog **GrafanaLoki** con `http://<ese host>:3100` (ingesta Loki; Grafana en `:3000` es solo la UI).

Supuestos por defecto de RabbitMQ:

- La app publica eventos de pacientes al exchange `patients`.
- El worker consume eventos de planes alimentarios desde la cola `ms-patients-queue`.
- Los routing keys esperados son `patient.created`, `patient.updated`, `meal-plan.created` y `meal-plan.updated`.
- Si tu broker externo usa otros nombres, sobrescríbelos con las variables anteriores.

Observabilidad externa:

- La URL de Grafana por sí sola no alcanza para configurar la app.
- Para logs vía variables simples, puedes usar `LOKI_URL` y/o `SEQ_SERVER_URL`.
- Para trazas vía variables estándar de OpenTelemetry, usa `OTEL_EXPORTER_OTLP_ENDPOINT`.
- Alternativa compatible con ms-logistic: `Telemetry__OtlpEndpoint` (ya se define desde `INFRA_HOST` en compose).
- Para dashboards de métricas, el endpoint `/metrics` ya queda expuesto por la API; normalmente un `Prometheus` externo hará el scrape.

### Despliegue en servidor

1. Copia el repositorio (o artefactos) al servidor.
2. Comprueba la red: `docker network ls` — debe existir **`develop_nutricenter_net`** (si no: `docker network create develop_nutricenter_net`).
3. Define `.env` en `/root/api_patient/.env` o configura las variables equivalentes en GitHub Actions.
4. Sube la app: `docker compose -f docker-compose-dev.yml up -d --build` o el workflow **deploy-ssh**.
5. Ajusta en GitHub **Variables** `INFRA_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USERNAME`, `RABBITMQ_PASSWORD`, `RABBITMQ_VIRTUALHOST`, `LOKI_URL`, `OTEL_EXPORTER_OTLP_ENDPOINT`, `SEQ_SERVER_URL`, etc.
6. Si tu infraestructura externa usa nombres distintos para exchanges, colas o routing keys, define también las variables `RABBITMQ_FOODPLANS_EXCHANGE`, `RABBITMQ_FOODPLANS_QUEUE`, `RABBITMQ_FOODPLAN_CREATED_ROUTING_KEY`, `RABBITMQ_FOODPLAN_UPDATED_ROUTING_KEY`, `RABBITMQ_PATIENTS_EXCHANGE`, `RABBITMQ_PATIENT_CREATED_ROUTING_KEY` y `RABBITMQ_PATIENT_UPDATED_ROUTING_KEY`.
7. Cambia contraseñas por defecto y restringe exposición pública de puertos administrativos como `Grafana` o `RabbitMQ Management`.

El workflow **deploy-ssh** exporta `INFRA_HOST` (por defecto `rabbitmq`) para que Compose pueda interpolar `Telemetry__OtlpEndpoint` y el sink Loki como en ms-logistic. Puedes seguir sobreescribiendo con `OTEL_EXPORTER_OTLP_ENDPOINT`, `LOKI_URL` o `SEQ_SERVER_URL` si lo necesitas.
