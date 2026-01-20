# Pruebas de Integración del Sistema de Gestión de Pacientes

## Introducción

Hoy vamos a explorar las pruebas de integración que hemos desarrollado para el sistema de gestión de pacientes clínicos. Estas pruebas validan que toda la arquitectura de capas funcione correctamente de forma integrada: desde la presentación en los controladores, pasando por la lógica de aplicación con MediatR, hasta la persistencia en base de datos.

---

## ¿Qué son las Pruebas de Integración?

Las pruebas de integración son diferentes a las pruebas unitarias. Mientras que las pruebas unitarias prueban componentes individuales aislados, las pruebas de integración **comprueban que múltiples componentes trabajen juntos correctamente**.

En nuestro caso:
- **Pruebas unitarias**: Prueban handlers, servicios y entidades de dominio de forma aislada con mocks.
- **Pruebas de integración**: Prueban el flujo completo: cliente HTTP ? API REST ? aplicación ? base de datos ? respuesta.

---

## Arquitectura del Sistema

Nuestro sistema sigue un patrón de capas limpio:

```
HttpClient (Cliente)
    ?
Controllers (PatientsController, HistoriesController, etc.)
    ?
MediatR (CQRS)
    ?
Handlers (CreatePatientHandler, CreateHistoryHandler, etc.)
    ?
Domain Entities (Patient, History, Contact, etc.)
    ?
Repositories (PatientRepository, HistoryRepository)
    ?
DbContext (Entity Framework Core)
    ?
PostgreSQL Database
```

Las pruebas de integración validan esta cadena completa.

---

## Estrategia de las Pruebas de Integración

### 1. **Configuración**: HttpClient Base

Todas nuestras pruebas usan un `HttpClient` que apunta a `https://localhost:7134`. Esto significa que:
- **La API debe estar corriendo** en modo Debug en tu máquina local.
- Usamos `System.Text.Json` para serializar y deserializar las respuestas.
- No usamos mocks; es una prueba real contra la base de datos real.

```csharp
private readonly HttpClient _client = new()
{
    BaseAddress = new Uri("https://localhost:7134")
};
```

### 2. **Datos Dinámicos**

Para evitar conflictos entre pruebas (especialmente con unique constraints como `documentNumber`), generamos valores únicos usando timestamps:

```csharp
var documentNumber = $"DOC-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
```

Esto garantiza que cada ejecución de prueba crea nuevos registros sin colisionar.

---

## Flujos de Prueba

### Flujo 1: Crear Paciente ?

**Objetivo**: Crear un nuevo paciente y verificar que la API devuelva un ID válido con respuesta exitosa.

**Pasos**:
1. Construir payload JSON con datos del paciente (nombre, fecha nacimiento, tipo sangre, etc.)
2. Enviar POST a `/api/Patients`
3. Validar:
   - Status HTTP es 200 u 201
   - `isSuccess = true`, `isFailure = false`
   - `value` contiene un GUID válido
   - `error` está vacío (código = "", descripción = "")

**Importancia**: Es el primer paso de cualquier flujo. Si falla, no podemos continuar.

---

### Flujo 2: Obtener Paciente (Éxito) ?

**Objetivo**: Recuperar un paciente existente por su ID.

**Pasos**:
1. Crear un paciente (reutilizar método helper)
2. Enviar GET a `/api/Patients/GetById?PatientId={id}`
3. Validar:
   - Status HTTP es 200
   - `isSuccess = true`
   - `value` contiene los datos del paciente (firstName, lastName, documentNumber)
   - El ID coincide con el que creamos

**Importancia**: Valida que el repositorio lee correctamente de la BD y que los datos se persistieron.

---

### Flujo 3: Obtener Paciente (No Encontrado) ?

**Objetivo**: Verificar que el sistema devuelve un error 404 cuando se busca un paciente inexistente.

**Pasos**:
1. Enviar GET a `/api/Patients/GetById?PatientId={Guid.Empty}` (ID inválido)
2. Validar:
   - Status HTTP es 404 (Not Found)
   - `isSuccess = false`, `isFailure = true`
   - `error.code = "Patient.NotFound"`
   - `error.description` contiene mensaje en español: "Paciente no encontrado"
   - `error.type` es 3 (NotFound)

**Importancia**: Valida que el error handling funciona correctamente y que el API devuelve códigos HTTP semánticos.

---

### Flujo 4: Crear Contacto del Paciente ?

**Objetivo**: Crear un contacto (dirección, teléfono) para un paciente existente.

**Pasos**:
1. Crear paciente (obtenemos patientId)
2. Enviar POST a `/api/Patients/Contacts` con:
   - patientId
   - direction: "Av. Siempre Viva 742"
   - reference: "Frente al colegio"
   - phoneNumber: "777-9999"
   - floor: "2"
   - coords: "-17.7833,-63.1821" (latitud, longitud)
3. Validar:
   - Status HTTP es 200 u 201
   - `isSuccess = true`
   - `value` contiene un GUID válido (contactId)

**Importancia**: Valida que el agregado Patient puede tener múltiples contactos. Es una relación uno-a-muchos.

---

### Flujo 5: Crear Plan Alimenticio ?

**Objetivo**: Crear un plan alimenticio que será usado en historias clínicas.

**Pasos**:
1. Enviar POST a `/api/FoodPlans` con:
   - name: "Plan-{timestamp}"
2. Validar:
   - Status HTTP es 200 u 201
   - `isSuccess = true`
   - `value` contiene un GUID válido (foodPlanId)

**Importancia**: Los planes alimenticios son referencias que usaremos en las historias clínicas.

---

### Flujo 6: Crear Historia Clínica ?

**Objetivo**: Crear una historia clínica para un paciente con un plan alimenticio específico.

**Pasos**:
1. Crear paciente ? obtenemos patientId
2. Crear plan alimenticio ? obtenemos foodPlanId
3. Enviar POST a `/api/Histories` con:
   - patientId
   - foodPlanId
   - reason: "Consulta inicial"
   - diagnostic: "Diagnóstico preliminar"
   - treatment: "Tratamiento recomendado"
4. Validar:
   - Status HTTP es 200 u 201
   - `isSuccess = true`
   - `value` contiene un GUID válido (historyId)

**Importancia**: La historia clínica es el agregado central. Contiene:
- Antecedentes médicos
- Evoluciones clínicas
- Órdenes médicas

---

### Flujo 7: Flujo Completo (End-to-End) ??

**Objetivo**: Validar que todo el sistema funciona en conjunto.

**Pasos**:
1. ? Crear Paciente
2. ? Verificar que el paciente existe (GET)
3. ? Crear un Contacto para el paciente
4. ? Crear un Plan Alimenticio
5. ? Crear una Historia Clínica vinculando paciente + plan
6. ? Verificar que la historia existe (GET)
7. ? Validar que historyId tiene patientId y foodPlanId correctos

```
Paciente
  ?? Contacto 1 (dirección, teléfono, coordenadas)
  ?? Historia Clínica
      ?? Plan Alimenticio
      ?? Antecedentes (se pueden agregar)
      ?? Evoluciones (se pueden agregar)
```

**Importancia**: Este es el test más crítico. Si falla, significa que uno o más componentes no funciona. Nos ayuda a identificar dónde está el problema.

---

## Ventajas de Estas Pruebas

1. **Confianza Real**: Ejecutan código real contra una BD real, no simulaciones.
2. **Detección de Errores de Integración**: Encuentran problemas que los tests unitarios no pueden (serialización, consultas SQL, transacciones).
3. **Documentación Viva**: El código de prueba sirve como documentación de cómo usar la API.
4. **Regresiones**: Previenen que cambios futuros rompan el sistema.

---

## Cómo Ejecutar las Pruebas

### Requisitos Previos:
1. Tener PostgreSQL corriendo
2. Ejecutar migraciones de Entity Framework Core:
   ```bash
   dotnet ef database update
   ```
3. Iniciar la API en modo Debug:
   ```bash
   dotnet run
   ```
   Esperamos ver: `Now listening on: https://localhost:7134`

### Ejecutar las pruebas:
```bash
dotnet test patient.test/IntegrationTest/PatientIntegrationTests.cs
```

### Interpretar resultados:
- ? Verde: La prueba pasó, el flujo funciona correctamente.
- ? Rojo: La prueba falló, hay un problema en alguna capa.

Si una prueba falla, el mensaje de error te dirá exactamente dónde:
```
Assert.True(response.StatusCode == HttpStatusCode.OK || ...)
Expected: True
Actual: False
Response Status: 404
```

---

## Casos de Error Comunes

| Problema | Causa Posible | Solución |
|----------|---------------|----------|
| `Connection refused` | La API no está corriendo | Ejecutar `dotnet run` en el proyecto api_patient |
| `404 Not Found en POST` | Ruta incorrecta o controller no mapeado | Verificar las rutas en los controllers |
| `500 Internal Server Error` | Excepción en la lógica de negocio | Ver logs de la consola, revisar handler |
| `Timeout` | La BD es lenta o no responde | Verificar conexión PostgreSQL, aumentar timeout |
| `Constraint violation` | Duplicado en documentNumber | Usar timestamps para generar valores únicos |

---

## Extensiones Futuras

Podríamos agregar más pruebas de integración para:

- ? **Actualización**: PATCH/PUT a `/api/Patients`, `/api/Histories`
- ? **Eliminación**: DELETE de contactos, antecedentes, evoluciones
- ? **Filtrado**: GET con parámetros de búsqueda, paginación
- ? **Validaciones**: Intencionalmente enviar datos inválidos (edad futura, documentNumber vacío)
- ? **Performance**: Pruebas de carga, tiempos de respuesta
- ? **Concurrencia**: Dos clientes actualizando el mismo paciente simultáneamente

---

## Conclusión

Las pruebas de integración son el puente entre el mundo de la teoría (tests unitarios aislados) y la realidad (usuarios reales usando la API). 

Ejecutando estas pruebas regularmente, especialmente antes de hacer deploy, nos aseguramos que:

1. El sistema funciona end-to-end ?
2. Los cambios no rompen funcionalidad existente ?
3. Los errores se manejan correctamente ?
4. La API responde con los códigos HTTP semánticos ?

**¡Gracias por ver! Ahora ejecuta las pruebas y valida que tu sistema funciona correctamente.** ??
