# Estructura del proyecto api_patient

Documento de referencia con la estructura de carpetas y proyectos de la solución.

---

## 1. Vista general de la solución

La solución **api_patient** es una API .NET 8 organizada en **Clean Architecture** (capas: dominio, aplicación, infraestructura, presentación). Incluye un Worker Service para procesamiento en segundo plano y un proyecto de pruebas.

| Proyecto | Descripción | Referencias |
|----------|-------------|-------------|
| **api_patient** | API web (presentación) | patient.application, patient.infrastructure |
| **patient.domain** | Entidades, eventos y reglas de negocio | — |
| **patient.application** | Casos de uso, CQRS (MediatR) | patient.domain |
| **patient.infrastructure** | Persistencia, integraciones, seguridad | patient.application, patient.domain |
| **patient.WorkerService** | Servicio en segundo plano (Outbox, RabbitMQ) | patient.application, patient.infrastructure |
| **patient.test** | Tests unitarios, de contrato e integración | patient.application, patient.domain |

**Archivo de solución:** `api_patient.sln`

---

## 2. Árbol de carpetas (excluye obj/bin/.git)

```
api_patient/
├── .github/
│   └── workflows/
├── .vscode/
│   ├── launch.json
│   └── tasks.json
├── api_patient/                    # Proyecto: API Web
│   ├── Auth/
│   ├── Controllers/
│   │   ├── FoodPlansController.cs
│   │   ├── HistoriesController.cs
│   │   └── PatientsController.cs
│   ├── Docs/
│   │   └── Postman/
│   │       ├── Patient-IntegrationTest.postman_collection.json
│   │       └── Patient-test.postman_environment.json
│   ├── Extensions/
│   │   ├── ControllerExtensions.cs
│   │   ├── MigrationExtension.cs
│   │   └── ResultExtensions.cs
│   ├── Middleware/
│   │   └── ResultFormatMiddleware.cs
│   ├── Options/
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs
│   ├── api_patient.csproj
│   ├── api_patient.http
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── patient.domain/                 # Proyecto: Dominio
│   ├── Abstractions/
│   │   ├── AgregateRoot.cs
│   │   ├── DomainEvent.cs
│   │   ├── Entity.cs
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Entities/
│   │   ├── Backgrounds/
│   │   │   └── Background.cs
│   │   ├── Contacts/
│   │   │   └── Contact.cs
│   │   ├── Evolutions/
│   │   │   └── Evolution.cs
│   │   ├── FoodPlans/
│   │   │   ├── FoodPlan.cs
│   │   │   └── IFoodPlanRepository.cs
│   │   ├── Histories/
│   │   │   ├── Events/
│   │   │   │   ├── BackgroundCreateEvent.cs
│   │   │   │   └── EvolutionCreateEvent.cs
│   │   │   ├── History.cs
│   │   │   └── IHistoryRepository.cs
│   │   └── Patients/
│   │       ├── Events/
│   │       │   ├── ContactCreateEvent.cs
│   │       │   ├── PatientCreatedEvent.cs
│   │       │   └── PatientUpdatedEvent.cs
│   │       ├── IPatientRepository.cs
│   │       ├── Patient.cs
│   │       └── PatientOutboxPayload.cs
│   ├── Results/
│   │   ├── DomainException.cs
│   │   ├── Error.cs
│   │   ├── ErrorType.cs
│   │   ├── Result.cs
│   │   └── ValidationError.cs
│   ├── Shared/
│   │   ├── BloodType.cs
│   │   ├── ClaimsConstants.cs
│   │   └── PagedResult.cs
│   ├── DependencyInjection.cs
│   └── patient.domain.csproj
│
├── patient.application/            # Proyecto: Aplicación (CQRS)
│   ├── FoodPlans/
│   │   └── CreateFoodPlan/
│   │       ├── CreateFoodPlanCommand.cs
│   │       └── CreateFoodPlandHandler.cs
│   ├── Histories/
│   │   ├── CreateHistory/
│   │   │   ├── CreateHistoryCommand.cs
│   │   │   └── CreateHistoryHandler.cs
│   │   ├── GetHistory/
│   │   │   ├── GetHistoryCommand.cs
│   │   │   └── GetHistoryHandler.cs
│   │   └── UpdateHistory/
│   │       ├── UpdateHistoryCommand.cs
│   │       └── UpdateHistoryHandler.cs
│   ├── Integration/
│   │   └── FoodPlans/
│   │       ├── SyncFoodPlanFromIntegrationCommand.cs
│   │       └── SyncFoodPlanFromIntegrationHandler.cs
│   ├── Patients/
│   │   ├── CreatePatient/
│   │   │   ├── CreatePatientCommand.cs
│   │   │   └── CreatePatientHandler.cs
│   │   ├── DeletePatient/
│   │   │   ├── DeletePatientCommand.cs
│   │   │   └── DeletePatientHandler.cs
│   │   ├── GetPatient/
│   │   │   ├── GetPatientCommand.cs
│   │   │   └── GetPatientHandler.cs
│   │   └── UpdatePatient/
│   │       ├── UpdatePatientCommand.cs
│   │       └── UpdatePatientHandler.cs
│   ├── DependencyInjection.cs
│   └── patient.application.csproj
│
├── patient.infrastructure/        # Proyecto: Infraestructura
│   ├── Extensions/
│   │   └── AuthenticationExtensions.cs
│   ├── Integration/
│   │   ├── FoodPlanEventConsumerHostedService.cs
│   │   ├── FoodPlanIntegrationEventDto.cs
│   │   ├── IPatientEventPublisher.cs
│   │   ├── PatientCreatedEventRabbitMqHandler.cs
│   │   ├── PatientEventPublishedDto.cs
│   │   ├── PatientEventRabbitMqPublisher.cs
│   │   ├── PatientUpdatedEventRabbitMqHandler.cs
│   │   ├── RabbitMqFoodPlanOptions.cs
│   │   └── RabbitMqPatientPublisherOptions.cs
│   ├── Migrations/
│   │   ├── 20251018055159_Initial.cs
│   │   ├── 20251018071211_Cambios.cs
│   │   ├── 20251115011741_ReMigration.cs
│   │   ├── 20251202002104_Contactos.cs
│   │   ├── 20260228140027_AddOutboxTable.cs
│   │   ├── DomainDbContextModelSnapshot.cs
│   │   └── PersistenceDbContextModelSnapshot.cs
│   ├── Percistence/
│   │   ├── DomainModel/
│   │   │   ├── Config/
│   │   │   │   ├── FoodPlanConfig.cs
│   │   │   │   ├── HistoryConfig.cs
│   │   │   │   └── PatientConfig.cs
│   │   │   └── DomainDbContext.cs
│   │   ├── Outbox/
│   │   │   ├── OutboxDatabase.cs
│   │   │   └── OutboxMessageHandler.cs
│   │   ├── PersistenceModel/
│   │   │   ├── Entities/
│   │   │   │   ├── BackgroundPM.cs
│   │   │   │   ├── ContactPM.cs
│   │   │   │   ├── EvolutionPM.cs
│   │   │   │   ├── FoodPlanPM.cs
│   │   │   │   ├── HistoryPM.cs
│   │   │   │   └── PatientPM.cs
│   │   │   ├── PersistenceDbContext.cs
│   │   │   └── (PersistenceModel)
│   │   ├── Repositories/
│   │   │   ├── FoodPlanRepository.cs
│   │   │   ├── HistoryRepository.cs
│   │   │   └── PatientRepository.cs
│   │   ├── IDatabase.cs
│   │   └── UnitOfWork.cs
│   ├── Security/
│   │   └── JwtSettings.cs
│   ├── DependencyInyection.cs
│   └── patient.infrastructure.csproj
│
├── patient.WorkerService/         # Proyecto: Worker (Outbox + RabbitMQ)
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs
│   ├── patient.WorkerService.csproj
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── patient.test/                  # Proyecto: Tests
│   ├── ContractTest/
│   │   ├── ContractDTOs/
│   │   │   ├── PatientContractDto.cs
│   │   │   └── ResponseContractDto.cs
│   │   └── PatientContractTest.cs
│   ├── CoverageHistory/
│   ├── IntegrationTest/
│   ├── UnitTest/
│   │   ├── Application/
│   │   │   ├── FoodPlanHandlerTest.cs
│   │   │   ├── HistoryHandlerTest.cs
│   │   │   └── PatientHandlerTest.cs
│   │   └── Domain/
│   │       ├── DomainDependencyInjectionTests.cs
│   │       ├── DomainResultsTests.cs
│   │       ├── HistoryBackgroudTest.cs
│   │       ├── HistoryEvolutionTest.cs
│   │       └── HistoryTest.cs
│   ├── ExecCodeCoverage.ps1
│   └── patient.test.csproj
│
├── docs/
│   ├── ESTRUCTURA_PROYECTO.md     # Este documento
│   └── PROMPT_ALIGN_API_SECURITY_RABBITMQ.md
│
├── api_patient.sln
├── docker-compose-dev.yml
├── .editorconfig
├── .gitattributes
└── .gitignore
```

---

## 3. Dependencias entre proyectos

```
                    ┌─────────────────────┐
                    │   patient.domain    │
                    │   (sin dependencias)│
                    └──────────┬──────────┘
                               │
                    ┌──────────▼──────────┐
                    │ patient.application │
                    └──────────┬──────────┘
                               │
           ┌───────────────────┼───────────────────┐
           │                   │                   │
           ▼                   ▼                   ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ patient.infrastructure │ │  api_patient   │ │ patient.WorkerService │
│ (EF, RabbitMQ, JWT)    │ │  (API Web)    │ │ (Outbox, consumer)    │
└──────────┬─────────────┘ └──────────────────┘ └──────────────────┘
           │
           └──────────────┬───────────────────────┘
                          │
               ┌──────────▼──────────┐
               │   patient.test      │
               │ (application+domain)│
               └─────────────────────┘
```

- **api_patient** y **patient.WorkerService** dependen de application e infrastructure.
- **patient.test** solo referencia application y domain (no infrastructure ni API) para tests unitarios/contrato.

---

## 4. Resumen por proyecto

### 4.1 api_patient (API Web)
- **Controllers:** Patients, Histories, FoodPlans.
- **Extensions:** registro de controladores, migraciones, resultados.
- **Middleware:** formato de respuestas (Result).
- **Configuración:** appsettings, launchSettings, Docker.

### 4.2 patient.domain
- **Abstractions:** Entity, AgregateRoot, DomainEvent, IRepository, IUnitOfWork.
- **Entities:** Patient, Contact, History, Evolution, Background, FoodPlan; eventos de dominio y repositorios (interfaces).
- **Results:** Result, Error, ErrorType, ValidationError, DomainException.
- **Shared:** PagedResult, BloodType, ClaimsConstants.

### 4.3 patient.application
- **CQRS por entidad:** Patients (CRUD), Histories (Create/Get/Update), FoodPlans (Create).
- **Integration:** sincronización de FoodPlans desde mensajería (SyncFoodPlanFromIntegration).

### 4.4 patient.infrastructure
- **Percistence:** DomainDbContext (dominio), PersistenceDbContext (tablas de persistencia), configuraciones EF, repositorios, UnitOfWork, Outbox.
- **Integration:** publicador y consumidores RabbitMQ (pacientes y planes de alimentación).
- **Security:** JWT (JwtSettings, AuthenticationExtensions).
- **Migrations:** EF Core para ambos contextos (dominio y persistencia), incl. tabla Outbox.

### 4.5 patient.WorkerService
- **Host:** Worker que registra Application, Infrastructure y consumidor RabbitMQ de FoodPlans.
- **Outbox:** servicio en segundo plano para procesar eventos de dominio (Joseco.Outbox.EFCore).
- **Configuración:** reutiliza appsettings de api_patient cuando está disponible.

### 4.6 patient.test
- **UnitTest/Application:** handlers de Patient, History, FoodPlan.
- **UnitTest/Domain:** resultados, inyección de dependencias, historias y evoluciones.
- **ContractTest:** DTOs de contrato y PatientContractTest.
- **Coverage:** ExecCodeCoverage.ps1 y carpeta CoverageHistory.

---

## 5. Archivos de configuración en raíz

| Archivo | Uso |
|---------|-----|
| `api_patient.sln` | Solución con los 6 proyectos. |
| `docker-compose-dev.yml` | Servicios para desarrollo (API, BD, RabbitMQ, etc.). |
| `.editorconfig` | Estilo de código. |
| `.gitignore` / `.gitattributes` | Control de versiones. |

---

*Documento generado a partir de la estructura actual del repositorio api_patient. Actualizar cuando se añadan proyectos o carpetas relevantes.*
