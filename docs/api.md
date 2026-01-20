# API Reference - Streaming Service

## 📡 Endpoints REST

### Base URL
- **Desarrollo**: `http://localhost:7001`
- **Swagger UI**: `http://localhost:7001/swagger`
- **Health Check**: `GET /health` → `200 OK`

### Autenticación
Todos los endpoints protegidos requieren JWT Bearer token en el header:
```
Authorization: Bearer <token>
```

---

## 🎬 Gestión de Sesiones

### 1. Crear Sesión de Streaming

Crea una nueva sesión de streaming programada para un evento.

**Endpoint**: `POST /api/streaming/session`

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "scheduledStartTime": "2026-01-20T18:00:00Z",
  "maxViewers": 1000
}
```

**Response**: `200 OK`
```json
{
  "sessionId": "7b8c9d01-2345-6789-abcd-ef0123456789"
}
```

**Casos de Error**:
- `400 Bad Request` - Datos inválidos (fecha pasada, maxViewers negativo)
- `500 Internal Server Error` - Error de persistencia

---

### 2. Obtener Acceso al Stream

Valida el acceso de un usuario autenticado a una sesión de streaming y retorna la URL del stream.

**Endpoint**: `GET /api/streaming/session/{eventId}/access`

**Headers**:
```
Authorization: Bearer <jwt_token>
```

**Path Parameters**:
- `eventId` (guid) - ID del evento

**Response**: `200 OK`
```json
{
  "streamUrl": "https://storage.example.com/streams/7b8c9d01-2345-6789-abcd-ef0123456789/stream.m3u8",
  "sessionId": "7b8c9d01-2345-6789-abcd-ef0123456789",
  "expiresAt": "2026-01-20T22:00:00Z",
  "quality": "1080p"
}
```

**Casos de Error**:
- `401 Unauthorized` - Token inválido o userId faltante
```json
{
  "message": "Invalid or missing user ID in token"
}
```

- `400 Bad Request` - Diversos casos de negocio:
```json
{
  "message": "No active streaming session found for this event"
}
```
```json
{
  "message": "Access not yet available. Stream opens at 17:50 UTC"
}
```
```json
{
  "message": "No valid access token found. Please check your reservation"
}
```
```json
{
  "message": "Stream capacity reached. You are in the waiting queue"
}
```

**Reglas de Negocio**:
- Usuario puede acceder 10 minutos antes del inicio programado
- Sesión debe estar en estado `Live` o `Scheduled`
- Usuario debe tener un registro de acceso válido (reservación confirmada)
- Capacidad máxima de espectadores no debe estar excedida

---

## 🔑 Gestión de Tokens

### 3. Generar Token de Acceso

Genera un token interno de acceso al stream para un usuario con reservación confirmada.

**Endpoint**: `POST /api/streaming/token`

**Request Body**:
```json
{
  "sessionId": "7b8c9d01-2345-6789-abcd-ef0123456789",
  "userId": "9f1e2d3c-4b5a-6789-0abc-def123456789",
  "reservationId": "4c5d6e7f-8a9b-0c1d-2e3f-456789abcdef"
}
```

**Response**: `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rt_9f8e7d6c5b4a3210fedcba0987654321",
  "expiresAt": "2026-01-20T19:00:00Z",
  "refreshTokenExpiresAt": "2026-01-27T18:00:00Z",
  "userId": "9f1e2d3c-4b5a-6789-0abc-def123456789",
  "sessionId": "7b8c9d01-2345-6789-abcd-ef0123456789"
}
```

**Propiedades del Token**:
- `token` - JWT con expiración de 60 minutos (configurable)
- `refreshToken` - Token para renovar, válido 7 días
- `expiresAt` - Timestamp UTC de expiración del token principal
- `refreshTokenExpiresAt` - Timestamp UTC de expiración del refresh token

---

### 4. Refrescar Token

Renueva un token expirado usando el refresh token.

**Endpoint**: `POST /api/streaming/refresh-token`

**Request Body**:
```json
{
  "expiredToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rt_9f8e7d6c5b4a3210fedcba0987654321"
}
```

**Response**: `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rt_1a2b3c4d5e6f7890abcdef1234567890",
  "expiresAt": "2026-01-20T20:00:00Z",
  "refreshTokenExpiresAt": "2026-01-27T19:00:00Z",
  "userId": "9f1e2d3c-4b5a-6789-0abc-def123456789",
  "sessionId": "7b8c9d01-2345-6789-abcd-ef0123456789"
}
```

**Casos de Error**:
- `400 Bad Request` - Refresh token expirado o inválido
- `401 Unauthorized` - Token principal no corresponde al refresh token

---

### 5. Validar Token de Acceso

Valida un token de acceso y retorna la URL del stream asociada.

**Endpoint**: `GET /api/streaming/validate`

**Query Parameters**:
- `token` (string) - Token de acceso a validar

**Ejemplo**: `/api/streaming/validate?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

**Response**: `200 OK`
```json
{
  "streamUrl": "https://storage.example.com/streams/7b8c9d01/stream.m3u8",
  "isEncrypted": false
}
```

**Casos de Error**:
- `400 Bad Request` - Token inválido o expirado

---

## 📺 Endpoints de Streaming (Simulados)

### 6. Obtener Patrón de Streaming

Retorna información del punto de entrada para reproductores de video (HLS/DASH).

**Endpoint**: `GET /api/streaming/stream/{eventId}/{token}`

**Path Parameters**:
- `eventId` (guid) - ID del evento
- `token` (string) - Token de acceso validado

**Response**: `200 OK`
```json
{
  "type": "HLS",
  "manifestUrl": "/api/streaming/mock/7b8c9d01-2345-6789-abcd-ef0123456789/playlist.m3u8?token=eyJhbGci...",
  "licenseUrl": "/api/streaming/mock/license",
  "metadata": {
    "title": "Evento en Vivo",
    "resolution": "1080p",
    "framerate": 60,
    "isLive": true
  }
}
```

**Uso**: Frontend puede usar esta información para inicializar reproductores como Video.js, HLS.js, Shaka Player.

---

### 7. Obtener Playlist HLS (Mock)

Retorna una playlist M3U8 simulada para testing de reproductores.

**Endpoint**: `GET /api/streaming/mock/{eventId}/playlist.m3u8`

**Query Parameters**:
- `token` (string) - Token de acceso

**Response**: `200 OK`
```
Content-Type: application/vnd.apple.mpegurl

#EXTM3U
#EXT-X-VERSION:3
#EXT-X-TARGETDURATION:10
#EXT-X-MEDIA-SEQUENCE:0
#EXTINF:10.0,
/api/streaming/mock/7b8c9d01-2345-6789-abcd-ef0123456789/chunk_0.ts
#EXTINF:10.0,
/api/streaming/mock/7b8c9d01-2345-6789-abcd-ef0123456789/chunk_1.ts
#EXT-X-ENDLIST
```

**Nota**: Este endpoint es para desarrollo/testing. En producción, debe apuntar a un CDN real.

---

## 🔴 SignalR Hub - Tiempo Real

### Hub Path
`/streamingHub`

### Autenticación
Enviar JWT token en query string al conectar:
```
/streamingHub?access_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Métodos Invocables desde el Cliente

#### 1. JoinSession
Unirse a una sesión de streaming.

**Parámetros**:
- `sessionId` (string) - ID de la sesión
- `capacity` (number) - Capacidad máxima de la sesión

**Ejemplo (JavaScript)**:
```javascript
await connection.invoke("JoinSession", "7b8c9d01-2345-6789-abcd-ef0123456789", 1000);
```

**Respuestas del Servidor**:
- `AccessGranted` - Usuario admitido a la sesión
- `ReceiveError` - Capacidad llena, usuario en cola de espera
- `ViewerCountUpdated` - Contador actualizado para todo el grupo

---

#### 2. LeaveSession
Salir de una sesión de streaming.

**Parámetros**:
- `sessionId` (string) - ID de la sesión

**Ejemplo**:
```javascript
await connection.invoke("LeaveSession", "7b8c9d01-2345-6789-abcd-ef0123456789");
```

**Efectos**:
- Decrementa el contador de espectadores
- Envía `ViewerCountUpdated` al grupo
- Notifica a la cola de espera con `SpaceAvailable`

---

#### 3. SendChatMessage
Enviar mensaje de chat a todos los usuarios de una sesión.

**Parámetros**:
- `sessionId` (string) - ID de la sesión
- `message` (string) - Texto del mensaje

**Ejemplo**:
```javascript
await connection.invoke("SendChatMessage", "7b8c9d01-2345-6789-abcd-ef0123456789", "¡Hola a todos!");
```

**Respuesta del Servidor** (broadcast a todo el grupo):
```javascript
// Evento: ReceiveChatMessage
{
  "username": "john.doe",
  "text": "¡Hola a todos!",
  "timestamp": "2026-01-20T18:30:00Z"
}
```

---

#### 4. SendSignal
Enviar señal genérica a todos los usuarios conectados.

**Parámetros**:
- `user` (string) - Identificador del usuario
- `signal` (string) - Contenido de la señal

**Ejemplo**:
```javascript
await connection.invoke("SendSignal", "user123", "ping");
```

**Respuesta del Servidor** (broadcast global):
```javascript
// Evento: ReceiveSignal
// Parámetros: user, signal
```

---

### Eventos Recibidos por el Cliente

| Evento | Descripción | Parámetros |
|--------|-------------|------------|
| `AccessGranted` | Acceso aprobado a la sesión | `message` (string) |
| `ReceiveError` | Error o capacidad llena | `message` (string) |
| `ViewerCountUpdated` | Contador de espectadores actualizado | `count` (number) |
| `ReceiveChatMessage` | Mensaje de chat recibido | `{ username, text, timestamp }` |
| `SpaceAvailable` | Espacio disponible en sesión (para cola) | `message` (string) |
| `ReceiveSignal` | Señal genérica recibida | `user` (string), `signal` (string) |

---

## 🔌 Ejemplo de Integración Completa

### Flujo: Usuario ve un stream

```javascript
import * as signalR from "@microsoft/signalr";

// 1. Obtener acceso al stream (REST)
const response = await fetch(`http://localhost:7001/api/streaming/session/${eventId}/access`, {
  headers: {
    'Authorization': `Bearer ${userJWT}`
  }
});

const { streamUrl, sessionId, expiresAt } = await response.json();

// 2. Conectar a SignalR Hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`http://localhost:7001/streamingHub?access_token=${userJWT}`)
  .withAutomaticReconnect()
  .build();

// 3. Suscribirse a eventos
connection.on("ViewerCountUpdated", (count) => {
  console.log(`Espectadores: ${count}`);
  updateViewerCounter(count);
});

connection.on("ReceiveChatMessage", (msg) => {
  console.log(`[${msg.username}]: ${msg.text}`);
  appendChatMessage(msg);
});

connection.on("AccessGranted", (msg) => {
  console.log("✅ Acceso concedido al stream");
  initializeVideoPlayer(streamUrl);
});

connection.on("ReceiveError", (err) => {
  console.warn("⚠️ Error:", err);
  showWaitingQueueUI();
});

// 4. Iniciar conexión y unirse a sesión
await connection.start();
await connection.invoke("JoinSession", sessionId, 1000);

// 5. Enviar mensajes de chat
document.getElementById("sendBtn").onclick = async () => {
  const message = document.getElementById("chatInput").value;
  await connection.invoke("SendChatMessage", sessionId, message);
};

// 6. Cleanup al salir
window.addEventListener("beforeunload", async () => {
  await connection.invoke("LeaveSession", sessionId);
  await connection.stop();
});
```

---

## 📊 Códigos de Estado HTTP

| Código | Descripción |
|--------|-------------|
| `200 OK` | Operación exitosa |
| `400 Bad Request` | Datos inválidos o regla de negocio violada |
| `401 Unauthorized` | Token ausente, inválido o expirado |
| `404 Not Found` | Recurso no encontrado |
| `500 Internal Server Error` | Error del servidor (logging habilitado) |

---

## 🔒 Notas de Seguridad

1. **Validación JWT**: Todos los endpoints protegidos validan firma, issuer, audience y expiración
2. **HTTPS en Producción**: Usar siempre HTTPS para evitar interceptación de tokens
3. **CORS**: Configurar `CORS__AllowedOrigins` con dominios específicos en producción
4. **Rate Limiting**: Considerar implementar rate limiting para prevenir abuso
5. **Token Rotation**: Usar refresh tokens y rotar tokens principales regularmente

---

**Última Actualización**: 2026-01-20  
**Versión de API**: v1
