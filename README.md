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

La aplicación (API, worker, PostgreSQL) vive en **`docker-compose-dev.yml`**. El repo también trae un stack local de referencia en **`docker-compose.observability.yml`**, pero no es obligatorio: la app puede conectarse a infraestructura externa de **RabbitMQ**, **Loki** y **OTLP** (Tempo/Jaeger). **No se usa `.env` ni `env_file`** en el compose de la app: RabbitMQ, Loki, OTLP y Consul se fijan en el YAML; la única interpolación permitida es **`${INFRA_HOST}`** (host del stack compartido).

### Arranque local recomendado

La red **`develop_nutricenter_net`** es **externa** en ambos composes. Créala si no existe (en el servidor ya debería existir):

```bash
docker network create develop_nutricenter_net
```

1. Define solo el host de infra compartida (misma máquina que Rabbit/Loki/OTLP/Consul). En PowerShell:

```powershell
$env:INFRA_HOST = "127.0.0.1"
```

En bash:

```bash
export INFRA_HOST=127.0.0.1
```

2. Aplicación:

```bash
docker compose -f docker-compose-dev.yml up -d --build
```

3. Observabilidad (desde la raíz del repo; este compose sigue pudiendo usar su propio `--env-file` solo para Grafana/Prometheus del stack de referencia):

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

Con el stack local de referencia, **api-patient** y **patient-worker** pueden enviar trazas OTLP y logs a Loki. En código, si **`Loki:Uri`** no es una URI válida o está vacía, no se activa el sink Grafana Loki y Serilog sigue con consola según `appsettings`. **`Telemetry:OtlpEndpoint`** configura el exportador OTLP (trazas y métricas).

En ejecución local con `dotnet run`, usa **`appsettings.Development.json`** (OTLP y Loki en `localhost`) o sobrescribe en configuración según necesites.

**Nota:** Grafana no recibe datos directamente de la app. Grafana consume datasources ya configurados, típicamente `Prometheus`, `Loki` y `Jaeger` o `Tempo`.

### Health checks y métricas (API)

- `GET /health/live` — liveness
- `GET /health/ready` — readiness (incluye PostgreSQL)
- `GET /metrics` — métricas Prometheus

## Infraestructura externa

Si en tu servidor solo desplegarás este proyecto y **no** levantarás `docker-compose.observability.yml`, el compose de la app ya fija **RabbitMQ** (`admin`/`admin`, puerto `5672`, vhost `/`), **Loki** (`:3100`), **OTLP** (`:4317`) y **Consul** (`:8500`) contra **`${INFRA_HOST}`**. No hace falta archivo `.env` para la app.

Con **`INFRA_HOST`**, `docker-compose-dev.yml` define entre otras:

- `RabbitMQ__HostName` = host de infra compartida (mismo valor que `INFRA_HOST`).
- `Telemetry__OtlpEndpoint` = `http://<INFRA_HOST>:4317`.
- `Loki__Uri` = `http://<INFRA_HOST>:3100`.
- En la API: `Consul__Host` = `http://<INFRA_HOST>:8500`, registro del servicio HTTP con health en `/health/live`.

**Infra en otro servidor que la API (caso habitual):** `INFRA_HOST` debe ser la **IP o DNS del servidor donde están RabbitMQ, Loki, OTLP y Consul** (no la del host solo de Postgres/API). Los contenedores `api-patient` y `patient-worker` abren conexiones **salientes** hacia ese host (puertos 5672, 3100, 4317, 8500); PostgreSQL sigue siendo local al `docker-compose-dev.yml`.

**Consul y health check:** el agente de Consul que recibe el registro ejecutará el check HTTP contra `Consul__ServiceAddress`. El nombre `api-patient` solo sirve si ese agente puede resolver y alcanzar ese host (por ejemplo, misma red Docker). Si **Consul está en otro servidor** que el de la API, sustituye en `docker-compose-dev.yml` `Consul__ServiceAddress` por la **IP o DNS del servidor donde despliegas la API** (vista desde el servidor de Consul), y permite tráfico **hacia** ese host en el puerto publicado de la API (`5002`).

Supuestos por defecto de RabbitMQ (editables en el YAML del repo si tu broker usa otra topología):

- La app publica eventos de pacientes al exchange `patients`.
- El worker consume eventos de planes alimentarios desde la cola `ms-meal-plans-queue`.
- Los routing keys esperados son `patient.created`, `patient.updated`, `meal-plan.created` y `meal-plan.updated`.

Observabilidad externa:

- Para dashboards de métricas, el endpoint `/metrics` queda expuesto por la API; normalmente un Prometheus externo hará el scrape.

### Despliegue en servidor

1. Copia el repositorio (o artefactos) al servidor, o usa el workflow **deploy-ssh**.
2. Comprueba la red: `docker network ls` — debe existir **`develop_nutricenter_net`** (si no: `docker network create develop_nutricenter_net`).
3. En el servidor o en GitHub Actions, exporta **`INFRA_HOST`** con la IP o hostname del stack compartido (RabbitMQ, Loki, OTLP, Consul). En GitHub Actions debe existir el secreto **`INFRA_HOST`** (no usar `vars` para este valor).
4. Sube la app: `docker compose -f docker-compose-dev.yml up -d --build` o el workflow **deploy-ssh**, que valida `secrets.INFRA_HOST` y los secretos SSH, sube un `deploy.tar.gz` y ejecuta `docker compose` con `INFRA_HOST` definido.
5. Cambia contraseñas por defecto en producción y restringe la exposición pública de puertos administrativos (Grafana, RabbitMQ Management, Consul, etc.).
