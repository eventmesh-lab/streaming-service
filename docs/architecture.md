# Arquitectura del Streaming Service

## 📐 Visión General

Este microservicio implementa **Arquitectura Hexagonal (Ports & Adapters)** con principios de **Domain-Driven Design (DDD)**, organizado en 4 capas principales:

```
┌─────────────────────────────────────────────────────────────┐
│                      streaming-service.Api                   │
│  (Controllers, SignalR Hubs, Middleware, Program.cs)        │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│                streaming-service.Application                 │
│    (Commands, Queries, Handlers - CQRS con MediatR)         │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│                  streaming-service.Domain                    │
│  (Entities, Value Objects, Ports/Interfaces, Domain Events) │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│              streaming-service.Infrastructure                │
│  (MongoDB, RabbitMQ, JWT, Implementaciones de Ports)        │
└─────────────────────────────────────────────────────────────┘
```

## 🔄 Flujo de Datos: De la Petición a la Respuesta

### Ejemplo: Usuario Solicita Acceso a un Stream

```
1. [Frontend] → HTTP GET /api/streaming/session/{eventId}/access
                + Header: Authorization Bearer <JWT>
                ↓
2. [Controller] StreamingController.GetStreamAccess()
   - Extrae userId del JWT (claims "sub" o "userId")
   - Extrae accessToken del header Authorization
   - Crea GetStreamAccessQuery(eventId, userId, accessToken)
                ↓
3. [MediatR] Enruta la query al handler correspondiente
                ↓
4. [Handler] GetStreamAccessQueryHandler.Handle()
   a) Consulta IStreamingSessionRepository para obtener sesión activa
   b) Valida timing (acceso 10 min antes del inicio programado)
   c) Verifica estado de la sesión (Live o Scheduled)
   d) Consulta IStreamingAccessRepository para validar acceso del usuario
   e) Verifica capacidad máxima de espectadores
   f) Genera/recupera la URL del stream
                ↓
5. [Repository] MongoStreamingSessionRepository / MongoStreamingAccessRepository
   - Ejecuta queries contra MongoDB
   - Retorna entidades de dominio (StreamingSession, StreamingAccess)
                ↓
6. [Handler] Construye StreamAccessResult
                ↓
7. [Controller] Retorna 200 OK con JSON:
   {
     "streamUrl": "https://...",
     "sessionId": "...",
     "expiresAt": "...",
     "quality": "1080p"
   }
```

### Flujo de Creación de Sesión

```
1. [Frontend] → HTTP POST /api/streaming/session
   Body: { eventId, scheduledStartTime, maxViewers }
                ↓
2. [Controller] → CreateSessionCommand via MediatR
                ↓
3. [Handler] CreateSessionHandler
   - Crea entidad StreamingSession (Domain)
   - Persiste via IStreamingSessionRepository
   - Publica SessionStartedEvent a RabbitMQ
                ↓
4. [Infrastructure] 
   - MongoStreamingSessionRepository guarda en MongoDB
   - RabbitMQMessagePublisher publica evento
                ↓
5. [RabbitMQ] Otros microservicios pueden consumir el evento
```

### Flujo de Comunicación en Tiempo Real (SignalR)

```
1. [Frontend] WebSocket connection → /streamingHub?access_token=<JWT>
                ↓
2. [SignalR Hub] StreamingHub
   - Valida JWT (configurado en Program.cs para aceptar query string)
   - Cliente invoca: JoinSession(sessionId, capacity)
                ↓
3. [Hub Logic]
   - Verifica capacidad con diccionario en memoria (_sessionViewers)
   - Si hay espacio: añade al grupo SignalR con ID = sessionId
   - Si capacidad llena: añade a grupo "Queue_{sessionId}"
                ↓
4. [Broadcasting]
   - Hub envía a grupo: ViewerCountUpdated(count)
   - Hub envía al caller: AccessGranted o ReceiveError
                ↓
5. [Chat] Cliente invoca: SendChatMessage(sessionId, message)
   - Hub broadcast a grupo: ReceiveChatMessage({ username, text, timestamp })
```

## 🔌 Dependencias Externas

### 1. MongoDB (Base de Datos Principal)

**Propósito**: Persistencia de sesiones, accesos y auditoría

**Colecciones**:
- `streaming_sessions` - Sesiones de streaming
- `streaming_accesses` - Tokens de acceso por usuario/sesión
- `audit_logs` - Registro de eventos y accesos

**Configuración**:
```
MongoDb__ConnectionString: mongodb://admin:pass@mongodb:27017
MongoDb__DatabaseName: streaming_db
```

**Uso en Código**:
- `MongoDbContext` - Inicializa conexión y colecciones
- `MongoStreamingSessionRepository` - CRUD de sesiones
- `MongoStreamingAccessRepository` - Gestión de accesos
- `MongoAuditLogger` - Escritura de logs de auditoría

### 2. RabbitMQ (Message Broker)

**Propósito**: Publicación de eventos de dominio para comunicación asíncrona entre microservicios

**Exchanges y Routing Keys**:
- `streaming-exchange` (tipo: Topic)
- `session.started` - Cuando se inicia una sesión
- `recording.completed` - Cuando finaliza una grabación

**Configuración**:
```
RabbitMQ__HostName: rabbitmq
RabbitMQ__Port: 5672
RabbitMQ__UserName: streaming_user
RabbitMQ__Password: streaming_pass_2024
```

**Uso en Código**:
- `RabbitMQMessagePublisher` - Implementa `IMessagePublisher`
- `ReservationConfirmedConsumer` - Consumidor de eventos externos (ej. de un servicio de reservaciones)

### 3. Keycloak / API Gateway (Implícito)

**Propósito**: Proveedor de identidad y generación de JWT

**Integración**:
- El servicio **no genera JWT de usuario** directamente
- Valida JWT recibidos en el header `Authorization: Bearer <token>`
- Extrae claims: `sub` o `userId` para identificar usuarios
- Configuración de validación en `Program.cs` con `JwtBearerDefaults`

**Configuración JWT**:
```
Jwt__Secret: super_secret_key_...
Jwt__Issuer: https://keycloak.eventmesh.com
Jwt__Audience: streaming-service
Jwt__ExpirationMinutes: 60
```

**Nota**: El servicio SÍ genera **AccessTokens internos** para acceso a streams mediante `ITokenGenerator` (implementado como `JwtTokenGenerator`).

### 4. CDN / Streaming Provider (Simulado)

**Propósito**: Entrega real de contenido de video (HLS/DASH)

**Estado Actual**: 
- URLs generadas son **simuladas** (`https://storage.example.com/streams/{sessionId}/stream.m3u8`)
- Endpoints mock en `/api/streaming/mock/{eventId}/playlist.m3u8` retornan playlists M3U8 de prueba

**Integración Futura**:
- Implementar `IStreamingRecordingService` (actualmente stub)
- Integrar con CDN real (AWS CloudFront, Azure Media Services, Cloudflare Stream)
- Generar URLs firmadas con tiempo de expiración

### 5. Otros Microservicios (Eventos Asincrónicos)

**Servicio de Reservaciones** (deducido):
- Publica evento `ReservationConfirmed` a RabbitMQ
- `ReservationConfirmedConsumer` lo consume y crea registros de acceso

**Posibles integraciones**:
- Servicio de Eventos (para obtener detalles de eventos)
- Servicio de Notificaciones (para alertas de inicio de stream)
- Servicio de Analytics (para métricas de visualización)

## 📊 Modelo de Datos

### Entidades Principales

#### 1. StreamingSession
Representa una sesión de transmisión en vivo programada o activa.

```csharp
{
  Id: Guid,
  EventId: Guid,                    // Relación con el evento externo
  StreamUrl: StreamingUrl?,         // URL del stream (puede ser null si no ha iniciado)
  Status: StreamingStatus,          // Scheduled, Live, Ended, Recorded
  ScheduledStartTime: DateTime,
  ActualStartTime: DateTime?,
  ActualEndTime: DateTime?,
  MaxViewers: int
}
```

**Métodos de Dominio**:
- `StartSession(StreamingUrl)` - Transición Scheduled → Live
- `EndSession()` - Transición Live → Ended
- `MarkAsRecorded()` - Transición Ended → Recorded

#### 2. StreamingAccess
Registro de acceso de un usuario a una sesión específica.

```csharp
{
  Id: Guid,
  SessionId: Guid,
  UserId: Guid,
  ReservationId: Guid,              // Referencia a la reservación original
  Token: AccessToken,               // Token de acceso interno
  Reconnections: List<DateTime>,    // Historial de accesos
  AccessCount: int,
  LastAccessAt: DateTime?
}
```

**Reglas de Negocio**:
- Máximo 3 reconexiones por hora
- `RecordAccess()` valida que el token no esté expirado
- `RotateToken()` permite refrescar el token

#### 3. StreamingRecording
Metadatos de una grabación completada.

```csharp
{
  Id: Guid,
  SessionId: Guid,
  StorageUrl: string,               // URL en sistema de almacenamiento
  FileSize: long,
  Duration: TimeSpan,
  CreatedAt: DateTime
}
```

### Value Objects

#### AccessToken
```csharp
{
  Token: string,                    // JWT
  RefreshToken: string,
  ExpiresAt: DateTime,
  RefreshTokenExpiresAt: DateTime,
  UserId: Guid,
  SessionId: Guid
}
```

**Propiedades Calculadas**:
- `IsExpired` - Verifica si el token principal expiró
- `IsRefreshExpired` - Verifica si el refresh token expiró

#### StreamingUrl
```csharp
{
  Value: string,
  IsEncrypted: bool
}
```

**Métodos**:
- `Encrypt(Func<string, string>)` - Aplica función de cifrado
- `Decrypt(Func<string, string>)` - Aplica función de descifrado

### Eventos de Dominio

#### SessionStartedEvent
Publicado cuando una sesión cambia a estado Live.

```csharp
{
  SessionId: Guid,
  EventId: Guid,
  StartedAt: DateTime
}
```

#### RecordingCompletedEvent
Publicado cuando se completa el procesamiento de una grabación.

```csharp
{
  RecordingId: Guid,
  SessionId: Guid,
  StorageUrl: string
}
```

## 🔐 Seguridad

### Autenticación y Autorización

1. **JWT Bearer Tokens**:
   - Validación en `Program.cs` con `AddJwtBearer()`
   - Issuer y Audience configurables
   - SigningKey simétrica (HMAC-SHA256)

2. **SignalR con JWT**:
   - Acepta token via query string `?access_token=...`
   - Configurado en evento `OnMessageReceived`

3. **Access Tokens Internos**:
   - Generados por `JwtTokenGenerator`
   - Incluyen claims: userId, sessionId, reservationId
   - Expiración configurable (default: 60 minutos)
   - Refresh token con expiración de 7 días

### Control de Acceso

- Verificación de userId en claims JWT
- Validación de reservación en base de datos
- Control de capacidad máxima de sesión
- Límite de reconexiones (3 por hora)

## 🧩 Patrones de Diseño Implementados

1. **Hexagonal Architecture**: Separación de dominio y adaptadores
2. **CQRS**: Commands y Queries separados con MediatR
3. **Repository Pattern**: Abstracción de persistencia
4. **Domain Events**: Eventos publicados a RabbitMQ
5. **Value Objects**: Encapsulación de lógica (AccessToken, StreamingUrl)
6. **Factory Methods**: Creación de Value Objects con validación
7. **Dependency Injection**: Inyección via ASP.NET Core DI

## ⚠️ Deuda Técnica Detectada

### 1. Gestión de Estado en Memoria (StreamingHub)
**Problema**: `_sessionViewers` y `_sessionCapacity` son `ConcurrentDictionary` estáticos en el Hub.

**Riesgos**:
- Se pierden al reiniciar la aplicación
- No escala horizontalmente (múltiples instancias de la API)
- No sincronizado con MongoDB

**Solución Recomendada**:
- Usar Redis para contador de espectadores compartido
- Implementar backplane de SignalR (Redis o Azure SignalR Service)
- Sincronizar periódicamente con MongoDB

### 2. URLs de Streaming Simuladas
**Problema**: `GenerateSimulatedStreamUrl()` retorna URLs ficticias.

**Impacto**: No funciona con reproductores de video reales.

**Solución Recomendada**:
- Integrar con CDN real (CloudFront, Azure Media Services)
- Implementar firma de URLs con tiempo de expiración
- Agregar soporte para DRM (PlayReady, Widevine)

### 3. Servicios Stub
**Archivos**:
- `StubStreamingRecordingService.cs`
- `StubImageProcessor.cs`

**Problema**: Implementaciones vacías que siempre retornan éxito.

**Solución Recomendada**:
- Implementar procesamiento real de grabaciones (transcoding, almacenamiento)
- Integrar con servicio de procesamiento de imágenes (thumbnails, previews)

### 4. CORS Permisivo en Desarrollo
**Código** (`Program.cs`):
```csharp
.SetIsOriginAllowed((host) => true) // For dev
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials()
```

**Riesgo de Seguridad**: Permite cualquier origen en desarrollo.

**Solución**: Usar configuración condicional y limitar orígenes en producción.

### 5. Falta de Paginación en Queries
**Problema**: Endpoints como `GetStreamAccess` no tienen paginación.

**Impacto**: Puede causar problemas de rendimiento con muchos datos.

**Solución**: Implementar paginación en repositorios y queries.

### 6. Falta de Circuit Breaker / Retry Policies
**Problema**: No hay manejo de fallos transitorios en conexiones a MongoDB/RabbitMQ.

**Solución Recomendada**:
- Implementar Polly para retry policies
- Circuit breaker para proteger servicios externos
- Health checks más robustos

### 7. Código Muerto o No Utilizado
**Encontrado**:
- Comentarios en `Program.cs`: `// c.AddSignalRSwaggerGen(); // Removed as it requires extra package`
- Comentario en `DependencyInjection.cs`: `// services.AddScoped<ISignalRService, SignalRService>(); // Moved to API`

**Acción**: Limpiar comentarios obsoletos o documentar claramente por qué están comentados.

### 8. Ausencia de Validación de Entrada Robusta
**Problema**: No se observan atributos de validación (DataAnnotations) en Commands.

**Riesgo**: Datos inválidos pueden llegar hasta el dominio.

**Solución**:
- Agregar FluentValidation para validación de Commands/Queries
- Implementar middleware de validación global

---

**Última Actualización**: 2026-01-20  
**Revisión**: v1.0
