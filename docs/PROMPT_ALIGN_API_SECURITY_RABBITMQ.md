# Prompt para alinear api-security con las colas de ms-infrastructure

Copia y pega el siguiente bloque como instrucción para la IA que trabaje en el servicio **api-security**, para que consuma eventos de RabbitMQ usando las mismas convenciones y colas definidas en ms-infrastructure (y ya usadas por api_patient).

---

## INICIO DEL PROMPT (copiar desde aquí)

Debes alinear el servicio **api-security** con la configuración de RabbitMQ de **ms-infrastructure** ([definitions.json](https://github.com/JoseLCT/ms-infrastructure/blob/main/rabbitmq/definitions.json)), de forma que use **solo** las colas y exchanges ya definidos allí, sin crear colas propias.

### 1. Contexto de ms-infrastructure

- **Exchange:** `patients` (tipo topic, durable).
- **Cola asignada a api-security:** `ms-security-queue` (ya definida en definitions.json).
- **Binding:** exchange `patients` → cola `ms-security-queue` con routing key `patient.*` (incluye `patient.created` y `patient.updated`).
- **Conexión:** mismo broker; vhost `/`; usuario/contraseña según entorno (en Docker suele ser `admin` / `StrongPassword123`).

No declares ni crees exchanges ni colas en código; solo **consume** desde la cola `ms-security-queue`, que ya existe en el broker.

### 2. Qué debe hacer api-security

- **Consumir** desde la cola **`ms-security-queue`** los mensajes con routing key `patient.created` y `patient.updated` (publicados por api_patient al exchange `patients`).
- **No publicar** a RabbitMQ para estos eventos; api-security solo actúa como consumidor de eventos de paciente.
- Usar en `ConnectionFactory`: `HostName`, `Port`, `UserName`, `Password` y **`VirtualHost`** (valor `"/"`).

### 3. Configuración de opciones RabbitMQ (appsettings / variables de entorno)

Usar una sección `RabbitMQ` con al menos:

- `HostName` (en Docker: `rabbitmq` si el contenedor está en la misma red que el broker).
- `Port`: `5672`.
- `UserName` / `Password` (en desarrollo local `guest`/`guest`; en Docker según ms-infrastructure, ej. `admin`/`StrongPassword123`).
- `VirtualHost`: `"/"`.
- Nombre de la cola a consumir: `QueueName` o equivalente = **`ms-security-queue`**.
- Opcional, para resiliencia (como en api_patient y ms-logistic): `ReconnectDelaySeconds` (ej. 5), `MaxReconnectAttempts` (0 = infinitos).

No uses nombres de cola inventados (ej. `api_security.patients`); **solo** `ms-security-queue`.

### 4. Contrato del mensaje (payload desde api_patient)

Los eventos se publican en JSON con estructura similar a (PascalCase):

- `PatientId` (Guid)
- `Id` (Guid del evento)
- `OccurredOn` (DateTime, ISO 8601)
- `FirstName`, `MiddleName`, `LastName`, `DocumentNumber`

Deserializa según este contrato para crear o actualizar el usuario/paciente en api-security (por ejemplo vincular `PatientId` al usuario).

### 5. Docker Compose (si aplica)

- El contenedor de api-security debe estar en la **misma red** que RabbitMQ (p. ej. red externa de ms-infrastructure) para resolver el hostname `rabbitmq`.
- Variables de entorno sugeridas (ajustar nombres si tu app usa otros):

```yaml
RabbitMQ__HostName: rabbitmq
RabbitMQ__Port: "5672"
RabbitMQ__UserName: admin
RabbitMQ__Password: StrongPassword123
RabbitMQ__VirtualHost: /
RabbitMQ__QueueName: ms-security-queue
# Opcional
RabbitMQ__ReconnectDelaySeconds: 5
RabbitMQ__MaxReconnectAttempts: 0
```

### 6. Resiliencia (recomendado)

- Usar **reintentos al arrancar** si RabbitMQ no está listo (delay entre intentos, ej. 5 s).
- En `ConnectionFactory`, activar **`AutomaticRecoveryEnabled = true`** y **`NetworkRecoveryInterval`** para reconexión automática si se cae el broker.

### 7. Resumen de alineación

| Concepto        | Valor en api-security                          |
|----------------|-------------------------------------------------|
| Cola a consumir| `ms-security-queue` (de ms-infrastructure)     |
| Exchange origen| `patients` (solo referencia; no declarar)       |
| Routing keys   | `patient.created`, `patient.updated`           |
| VirtualHost    | `"/"`                                           |
| Crear colas    | No; la cola ya existe en definitions.json      |

Implementa el consumer de RabbitMQ en api-security siguiendo lo anterior para que quede alineado con ms-infrastructure y con el patrón ya usado en api_patient y ms-logistic.

---

## FIN DEL PROMPT
