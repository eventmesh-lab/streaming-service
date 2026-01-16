# Guía de Consumo para Frontend — Streaming Service API

Esta guía resume los puntos clave para que un frontend consuma el microservicio de streaming, incluyendo REST, SignalR (tiempo real), autenticación y CORS.

## Base de Acceso
- **Base URL (dev):** `http://localhost:7001` (según Docker Compose / `API_HTTP_PORT`).
- **Swagger UI:** `http://localhost:7001/swagger`.
- **Healthcheck:** `GET /health` → `200 OK` si está operativo.

## Autenticación
- **Tipo:** JWT Bearer.
- **Header:** `Authorization: Bearer <token>`.
- **Claims necesarios:** se usa `sub` o `userId` para identificar al usuario en endpoints como acceso al stream.
- **SignalR:** permite enviar JWT vía query string: `access_token` al conectar con el hub.

Variables relevantes (de `.env.docker.example`):
- `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET` deben corresponder al proveedor de identidad (p.ej. Keycloak).

## CORS
- En desarrollo está abierto (`AllowAll`) y acepta **credenciales**.
- Para producción, define `CORS_ORIGINS` con los hosts permitidos y habilita solo métodos/headers necesarios.

## Endpoints REST
Base de ruta del controlador: `/api/streaming`

1) Crear sesión
- **POST** `/api/streaming/session`
- **Body:**
```json
{
  "eventId": "<GUID>",
  "scheduledStartTime": "2026-01-15T18:00:00Z",
  "maxViewers": 1000
}
```
- **200 OK:**
```json
{
  "SessionId": "<GUID>"
}
```

2) Generar token de acceso
- **POST** `/api/streaming/token`
- **Body:**
```json
{
  "sessionId": "<GUID>",
  "userId": "<GUID>",
  "reservationId": "<GUID>"
}
```
- **200 OK (AccessToken):**
```json
{
  "token": "<JWT>",
  "refreshToken": "<JWT/opaque>",
  "expiresAt": "2026-01-15T19:00:00Z",
  "refreshTokenExpiresAt": "2026-01-22T19:00:00Z",
  "userId": "<GUID>",
  "sessionId": "<GUID>"
}
```

3) Refrescar token
- **POST** `/api/streaming/refresh-token`
- **Body:**
```json
{
  "expiredToken": "<JWT>",
  "refreshToken": "<JWT/opaque>"
}
```
- **200 OK:** cuerpo igual al de `AccessToken` anterior.

4) Obtener acceso al stream
- **GET** `/api/streaming/session/{eventId}/access`
- **Headers:** `Authorization: Bearer <token>`
- **200 OK:**
```json
{
  "streamUrl": "https://cdn.example.com/live/event123/index.m3u8",
  "sessionId": "<GUID>",
  "expiresAt": "2026-01-15T19:00:00Z",
  "quality": "1080p"
}
```
- **401/400:**
```json
{ "message": "Invalid or missing user ID in token" }
```
o
```json
{ "message": "<detalle de error>" }
```

5) Validar token de acceso a la URL de streaming
- **GET** `/api/streaming/validate?token=<token>`
- **200 OK:**
```json
{ "StreamUrl": "https://.../index.m3u8", "IsEncrypted": false }
```

6) Patrón simulado de entrada para el reproductor (HLS)
- **GET** `/api/streaming/stream/{eventId}/{token}`
- **200 OK:**
```json
{
  "Type": "HLS",
  "ManifestUrl": "/api/streaming/mock/<eventId>/playlist.m3u8?token=<token>",
  "LicenseUrl": "/api/streaming/mock/license",
  "Metadata": {
    "Title": "Evento en Vivo",
    "Resolution": "1080p",
    "Framerate": 60,
    "IsLive": true
  }
}
```

7) Playlist HLS simulada
- **GET** `/api/streaming/mock/{eventId}/playlist.m3u8?token=<token>`
- **Content-Type:** `application/vnd.apple.mpegurl`
- **Contenido:** lista M3U8 con segmentos `chunk_X.ts` simulados.

## Tiempo Real (SignalR)
- **Hub Path:** `/streamingHub`
- **Autenticación:** enviar `access_token=<JWT>` en la query al conectar (o header Bearer si el cliente lo soporta).

### Eventos recibidos por el cliente
- `AccessGranted`: acceso autorizado tras `JoinSession`.
- `ReceiveError`: error (p.ej. capacidad llena → en cola).
- `ViewerCountUpdated`: número de espectadores actualizado (grupo de la sesión).
- `ReceiveChatMessage`: mensajes del chat del grupo.
- `SpaceAvailable`: notificación de espacio libre para la cola.
- `ReceiveSignal`: canal general de señales.

### Métodos del Hub (invocar desde el cliente)
- `JoinSession(sessionId: string, capacity: number)`
- `LeaveSession(sessionId: string)`
- `SendChatMessage(sessionId: string, message: string)`
- `SendSignal(user: string, signal: string)`

### Ejemplo de conexión (JS)
```js
import * as signalR from "@microsoft/signalr";

const token = "<JWT>";
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:7001/streamingHub?access_token=" + encodeURIComponent(token))
  .withAutomaticReconnect()
  .build();

connection.on("ViewerCountUpdated", (count) => console.log("viewers:", count));
connection.on("ReceiveChatMessage", (msg) => console.log(msg));
connection.on("AccessGranted", (msg) => console.log(msg));
connection.on("ReceiveError", (err) => console.warn(err));

await connection.start();
await connection.invoke("JoinSession", "<SESSION_ID>", 1000);
```

## Contratos y Formatos
- **Errores:** se devuelven como `{ "message": "..." }` en validaciones y fallos de acceso.
- **Tiempos:** fechas en UTC ISO-8601.
- **Seguridad:** usar HTTPS en producción; validar expiración (`expiresAt`) y refrescar tokens con `refreshToken`.

## Buenas Prácticas de Frontend
- Cachear `AccessToken` corto y gestionar `refreshToken` seguro.
- Reintentar conexión SignalR con backoff y suscripción a eventos clave.
- Validar `StreamUrl` con `validate` antes de inicializar el player.
- Configurar CORS del backend para los orígenes reales en producción.

## Configuración de Entorno (resumen)
- `API_HTTP_PORT`, `API_HTTPS_PORT` → puertos del servicio.
- `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET` → firma/validación de JWT.
- `CORS_ORIGINS` → hosts permitidos para el frontend.

## Referencias
- Swagger: `http://localhost:7001/swagger`
- Health: `http://localhost:7001/health`
