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

## Observabilidad (Serilog + Seq, OpenTelemetry + Jaeger, Prometheus, Grafana)

La aplicación (API, worker, PostgreSQL) vive en **`docker-compose-dev.yml`**. El stack RabbitMQ + observabilidad está en **`docker-compose.observability.yml`**. Ambos usan la red Docker externa **`develop_nutricenter_net`** (nombre real en el servidor cuando el proyecto Compose se llama `develop` y la red del YAML era `nutricenter_net`).

### Arranque local recomendado

La red **`develop_nutricenter_net`** es **externa** en ambos composes. Créala si no existe (en el servidor ya debería existir):

```bash
docker network create develop_nutricenter_net
```

1. Variables para OTLP + Seq (opcional pero recomendado si usas el stack de observabilidad):

```bash
cp docker-compose.env.example .env
```

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

### URLs y credenciales por defecto

| Servicio    | URL local típica     | Usuario / contraseña |
|------------|----------------------|----------------------|
| API        | `http://localhost:5002` | JWT según configuración |
| Grafana    | `http://localhost:3000`   | Con `--env-file monitoring/observability.env`: valores del archivo (ejemplo: `admin` / `CambiarGrafanaEnProduccion`). Sin env-file: `admin` / `admin` |
| Prometheus | `http://localhost:9090`   | Sin autenticación (restringir en producción con proxy o red) |
| Seq        | `http://localhost:5341`   | Primera visita: creas el usuario administrador en la UI de Seq |
| Jaeger UI  | `http://localhost:16686`  | Sin autenticación en `all-in-one` (no exponer a Internet sin protección) |

Con el archivo **`.env`** generado desde `docker-compose.env.example`, **api-patient** y **patient-worker** envían trazas OTLP gRPC a `http://jaeger:4317` y logs a Seq en `http://seq:80`. Si no usas `.env` (variables vacías), la aplicación sigue arrancando sin exportador OTLP ni sink Seq (solo consola según `appsettings`).

En ejecución local con `dotnet run`, puedes exportar definiendo en el sistema `OTEL_EXPORTER_OTLP_ENDPOINT` y `SEQ_SERVER_URL` solo cuando tengas Jaeger y Seq accesibles.

**Nota:** Grafana incluye datasources para Prometheus y Jaeger. Seq no trae un datasource oficial estable en Grafana; los logs se consultan en la propia UI de Seq.

### Health checks y métricas (API)

- `GET /health/live` — liveness
- `GET /health/ready` — readiness (incluye PostgreSQL)
- `GET /metrics` — métricas Prometheus

### Despliegue en servidor

1. Copia el repositorio (o artefactos) al servidor.
2. Comprueba la red: `docker network ls` — debe existir **`develop_nutricenter_net`** (si no: `docker network create develop_nutricenter_net`). RabbitMQ, Jaeger, Seq, Prometheus, etc. deben estar en esa red para que la API resuelva `rabbitmq`, `jaeger`, `seq` por nombre.
3. Opcional: `.env` en `/root/api_patient/.env`; el workflow **deploy-ssh** exporta por defecto OTLP → `http://jaeger:4317` y Seq → `http://seq:80` si no defines variables en GitHub.
4. Sube la app: `docker compose -f docker-compose-dev.yml up -d --build` o el workflow **deploy-ssh** (crea la red si falta y despliega en `/root/api_patient`).
5. Si **ya tienes** el stack de observabilidad en el mismo host y **misma red** `develop_nutricenter_net`, no necesitas `docker network connect`: los nuevos contenedores de la app se unen a esa red al hacer `up`. Solo usa `docker network connect develop_nutricenter_net <contenedor>` si algún servicio quedó en otra red.
6. Ajusta en GitHub **Variables** `RABBITMQ_HOSTNAME`, `RABBITMQ_USERNAME`, `RABBITMQ_PASSWORD`, etc., para que coincidan con tu RabbitMQ.
7. Si **Seq** reinicia en bucle, revisa `docker logs seq`.
8. Cambia contraseñas por defecto y restringe exposición pública de los puertos de observabilidad.

El workflow **deploy-ssh** solo sube `docker-compose-dev.yml`. El archivo `docker-compose.observability.yml` alinea la misma red para entornos nuevos o para recrear el stack completo desde el repo.
