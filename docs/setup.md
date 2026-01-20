# Guía de Configuración - Streaming Service

## 📋 Prerequisitos

### Software Requerido

- **Docker** 20.10+ y **Docker Compose** 2.0+
- **.NET SDK 8.0** (solo para desarrollo local sin Docker)
- **Git** para clonar el repositorio

### Puertos Utilizados

| Servicio | Puerto Host | Puerto Interno | Descripción |
|----------|-------------|----------------|-------------|
| Streaming API (HTTP) | 7001 | 8080 | Endpoint principal de la API |
| Streaming API (HTTPS) | 7002 | 8081 | Endpoint HTTPS (opcional) |
| MongoDB | 27020 | 27017 | Base de datos NoSQL |
| RabbitMQ AMQP | 5673 | 5672 | Message broker |
| RabbitMQ Management | 15673 | 15672 | Interfaz web de RabbitMQ |

**Nota**: Los puertos del host fueron modificados para evitar conflictos con instalaciones locales (MongoDB suele usar 27017, RabbitMQ 5672).

---

## 🔧 Variables de Entorno

### Archivo de Referencia
Copiar `.env.docker.example` a `.env` para Docker Compose:
```bash
cp .env.docker.example .env
```

### Tabla Completa de Variables

#### ASP.NET Core

| Variable | Valor por Defecto | Descripción |
|----------|-------------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Entorno de ejecución: `Development`, `Staging`, `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` | URLs de escucha (formato Kestrel) |

**Nota**: En producción, cambiar a `Production` para:
- Deshabilitar mensajes de error detallados
- Deshabilitar Swagger UI
- Habilitar optimizaciones de rendimiento

---

#### MongoDB

| Variable | Valor por Defecto | Descripción |
|----------|-------------------|-------------|
| `MongoDb__ConnectionString` | `mongodb://admin:streaming_pass_2024@mongodb:27017` | Cadena de conexión completa a MongoDB |
| `MongoDb__DatabaseName` | `streaming_db` | Nombre de la base de datos |
| `MONGO_INITDB_ROOT_USERNAME` | `admin` | Usuario root de MongoDB (solo Docker Compose) |
| `MONGO_INITDB_ROOT_PASSWORD` | `streaming_pass_2024` | Contraseña root de MongoDB (solo Docker Compose) |
| `MONGO_INITDB_DATABASE` | `streaming_db` | Base de datos inicial (solo Docker Compose) |

**Formato de ConnectionString**:
```
mongodb://[usuario]:[contraseña]@[host]:[puerto]/[database]?authSource=admin
```

**Colecciones Creadas**:
- `streaming_sessions` - Sesiones de transmisión
- `streaming_accesses` - Registros de acceso
- `audit_logs` - Auditoría de eventos

**Conexión Local** (sin Docker):
```
MongoDb__ConnectionString=mongodb://admin:streaming_pass_2024@localhost:27020
```

---

#### RabbitMQ

| Variable | Valor por Defecto | Descripción |
|----------|-------------------|-------------|
| `RabbitMQ__HostName` | `rabbitmq` | Hostname del broker (usar `localhost` si es local) |
| `RabbitMQ__Port` | `5672` | Puerto AMQP |
| `RabbitMQ__UserName` | `streaming_user` | Usuario de RabbitMQ |
| `RabbitMQ__Password` | `streaming_pass_2024` | Contraseña de RabbitMQ |
| `RabbitMQ__VirtualHost` | `/` | Virtual host (namespace) |
| `RABBITMQ_DEFAULT_USER` | `streaming_user` | Usuario por defecto (solo Docker Compose) |
| `RABBITMQ_DEFAULT_PASS` | `streaming_pass_2024` | Contraseña por defecto (solo Docker Compose) |

**Exchanges Utilizados**:
- `streaming-exchange` (tipo: Topic)

**Routing Keys**:
- `session.started` - Sesión iniciada
- `recording.completed` - Grabación completada
- `reservation.confirmed` (consumido desde otro servicio)

**Management UI**: http://localhost:15673
- Usuario: `streaming_user`
- Contraseña: `streaming_pass_2024`

---

#### JWT (JSON Web Tokens)

| Variable | Valor por Defecto | Descripción |
|----------|-------------------|-------------|
| `Jwt__Secret` | `super_secret_key_1234567890_min_length_32_chars_required` | Clave secreta para firmar tokens (mínimo 32 caracteres) |
| `Jwt__Issuer` | `https://keycloak.eventmesh.com` | Emisor del token (debe coincidir con Keycloak) |
| `Jwt__Audience` | `streaming-service` | Audiencia válida del token |
| `Jwt__ExpirationMinutes` | `60` | Duración del token principal en minutos |

**⚠️ IMPORTANTE - Seguridad**:
- **NUNCA** usar el secret por defecto en producción
- Generar un secret aleatorio de al menos 64 caracteres
- Usar variables de entorno o Azure Key Vault / AWS Secrets Manager
- Rotar secrets periódicamente

**Generar Secret Seguro**:
```bash
# Linux/Mac
openssl rand -base64 64

# PowerShell
[Convert]::ToBase64String((1..64|%{Get-Random -Max 256}))
```

**Validación**:
- El servicio valida `iss` (issuer), `aud` (audience), `exp` (expiration)
- Claims esperados: `sub` o `userId` para identificar al usuario

---

#### SignalR

| Variable | Valor por Defecto | Descripción |
|----------|-------------------|-------------|
| `SignalR__EnableDetailedErrors` | `true` | Habilitar errores detallados (solo desarrollo) |

**Configuración de Producción**:
```
SignalR__EnableDetailedErrors=false
```

**Escalabilidad**:
Para múltiples instancias de la API, implementar backplane con Redis:
```
SignalR__RedisConnectionString=redis-server:6379
```
(Requiere paquete `Microsoft.AspNetCore.SignalR.StackExchangeRedis`)

---

#### CORS

| Variable | Valor por Defecto | Descripción |
|----------|-------------------|-------------|
| `CORS__AllowedOrigins` | `http://localhost:5173,http://localhost:3000,https://frontend.eventmesh.com` | Orígenes permitidos (separados por coma) |

**Configuración de Desarrollo**:
```
CORS__AllowedOrigins=http://localhost:5173,http://localhost:3000
```

**Configuración de Producción**:
```
CORS__AllowedOrigins=https://app.example.com,https://www.example.com
```

**Comportamiento Actual** (`Program.cs`):
```csharp
// En desarrollo está configurado como "AllowAll" (permitir cualquier origen)
.SetIsOriginAllowed((host) => true)
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials()
```

**⚠️ CAMBIO REQUERIDO PARA PRODUCCIÓN**: Modificar `Program.cs` para usar orígenes específicos.

---

## 🐳 Docker

### Construcción de la Imagen

**Dockerfile** utiliza multi-stage build para optimizar tamaño:

```bash
# Construir imagen manualmente
docker build -t streaming-service:latest .

# Construir con tag específico
docker build -t streaming-service:1.0.0 .
```

**Etapas del Build**:
1. **build**: Restaura dependencias y compila la aplicación con SDK 8.0
2. **runtime**: Copia binarios compilados a imagen runtime más liviana (aspnet:8.0)

**Tamaño Estimado**:
- Imagen base SDK: ~700 MB
- Imagen final runtime: ~220 MB

**Optimizaciones Aplicadas**:
- Copia selectiva de archivos
- `--no-restore` en publish para evitar descarga redundante
- Instalación de curl solo en runtime para healthcheck

---

### Docker Compose

#### Servicios Definidos

1. **streaming-api**
   - Contexto: `.` (raíz del proyecto)
   - Puertos: 7001 (HTTP), 7002 (HTTPS)
   - Depende de: `mongodb`, `rabbitmq`
   - Health check: `curl http://localhost:8080/health`

2. **mongodb**
   - Imagen: `mongo:7.0`
   - Puerto: 27020
   - Volúmenes: `mongodb_data`, `mongodb_config`
   - Health check: `mongosh --eval "db.adminCommand('ping')"`

3. **rabbitmq**
   - Imagen: `rabbitmq:3.12-management`
   - Puertos: 5673 (AMQP), 15673 (Management)
   - Volumen: `rabbitmq_data`
   - Health check: `rabbitmq-diagnostics ping`

#### Comandos Esenciales

```bash
# Iniciar todos los servicios
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f streaming-api

# Ver estado de servicios
docker-compose ps

# Reiniciar solo la API
docker-compose restart streaming-api

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes (⚠️ borra datos)
docker-compose down -v

# Reconstruir imagen de la API
docker-compose build streaming-api
docker-compose up -d streaming-api
```

#### Troubleshooting

**Problema**: API no inicia, error de conexión a MongoDB
```bash
# Verificar que MongoDB esté healthy
docker-compose ps mongodb

# Ver logs de MongoDB
docker-compose logs mongodb

# Reiniciar MongoDB
docker-compose restart mongodb
```

**Problema**: RabbitMQ no acepta conexiones
```bash
# Verificar estado
docker-compose logs rabbitmq | grep "completed with"

# Esperar a que termine de iniciar (puede tomar 10-15 segundos)
# El healthcheck espera 10s antes de empezar a verificar
```

**Problema**: Puerto 7001 ya está en uso
```bash
# Cambiar puerto en docker-compose.yml
ports:
  - "7005:8080"  # Cambiar 7001 a 7005
```

---

## 🛠 Scripts de Desarrollo

### .NET CLI

#### Restaurar Dependencias
```bash
dotnet restore
```

#### Compilar
```bash
# Compilar todo el solution
dotnet build

# Compilar en modo Release
dotnet build -c Release
```

#### Ejecutar Tests
```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar con verbosidad
dotnet test -v normal

# Ejecutar solo tests de un proyecto
dotnet test tests/streaming-service.Application.Tests

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

#### Ejecutar la API Localmente
```bash
cd src/streaming-service.Api
dotnet run

# O con watch mode (recarga automática)
dotnet watch run
```

**Nota**: Asegurarse de tener MongoDB y RabbitMQ corriendo localmente o modificar `appsettings.Development.json`.

#### Publicar para Producción
```bash
dotnet publish -c Release -o ./publish
```

---

### Makefile (Opcional - No Incluido)

Sugerencia de `Makefile` para automatizar tareas comunes:

```makefile
.PHONY: build test run docker-up docker-down clean

build:
	dotnet build

test:
	dotnet test

run:
	cd src/streaming-service.Api && dotnet run

docker-up:
	docker-compose up -d

docker-down:
	docker-compose down

docker-rebuild:
	docker-compose build --no-cache
	docker-compose up -d

clean:
	dotnet clean
	rm -rf **/bin **/obj

restore:
	dotnet restore
```

**Uso**:
```bash
make build
make test
make docker-up
```

---

## 🔍 Health Checks

### Endpoint de Health
```bash
# Verificar estado de la API
curl http://localhost:7001/health

# Response: Healthy
```

### Health Checks Configurados

1. **API** (`/health`):
   - Verifica que la aplicación esté respondiendo
   - Intervalo: 30s, Timeout: 10s, Retries: 3

2. **MongoDB** (Docker):
   - Comando: `mongosh --eval "db.adminCommand('ping')"`
   - Intervalo: 10s, Timeout: 5s, Retries: 5

3. **RabbitMQ** (Docker):
   - Comando: `rabbitmq-diagnostics ping`
   - Intervalo: 10s, Timeout: 5s, Retries: 5

### Verificar Health de Contenedores
```bash
# Ver columna HEALTH de los contenedores
docker-compose ps

# Salida esperada:
# NAME               STATUS              HEALTH
# streaming-api      Up 30 seconds       healthy
# streaming-mongodb  Up 30 seconds       healthy
# streaming-rabbitmq Up 30 seconds       healthy
```

---

## 🌍 Configuración por Entorno

### Development (Local)

**appsettings.Development.json** (crear si no existe):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "MongoDb": {
    "ConnectionString": "mongodb://admin:streaming_pass_2024@localhost:27020",
    "DatabaseName": "streaming_db_dev"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5673,
    "UserName": "streaming_user",
    "Password": "streaming_pass_2024"
  },
  "Jwt": {
    "Secret": "development_secret_key_only_do_not_use_in_production_12345678",
    "Issuer": "http://localhost:8080",
    "Audience": "streaming-service-dev",
    "ExpirationMinutes": 120
  }
}
```

---

### Production

**Mejores Prácticas**:

1. **No incluir secretos en `appsettings.json`**
2. **Usar variables de entorno o servicios de secrets**:
   - Azure: Azure Key Vault
   - AWS: AWS Secrets Manager / Parameter Store
   - Kubernetes: Secrets
   - Docker: Docker Secrets

3. **Ejemplo con variables de entorno**:
```bash
export MongoDb__ConnectionString="mongodb+srv://..."
export Jwt__Secret="$(cat /run/secrets/jwt_secret)"
export RabbitMQ__Password="$(cat /run/secrets/rabbitmq_pass)"

dotnet streaming-service.Api.dll
```

4. **Kubernetes ConfigMap + Secret**:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: streaming-config
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  MongoDb__DatabaseName: "streaming_db"
  RabbitMQ__HostName: "rabbitmq-service"
---
apiVersion: v1
kind: Secret
metadata:
  name: streaming-secrets
type: Opaque
stringData:
  jwt-secret: "your-production-secret-here"
  mongo-connection: "mongodb://..."
```

---

## 🚀 Despliegue

### Docker Registry

```bash
# Tag para registry privado
docker tag streaming-service:latest myregistry.azurecr.io/streaming-service:1.0.0

# Push
docker push myregistry.azurecr.io/streaming-service:1.0.0
```

### Kubernetes (Ejemplo)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: streaming-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: streaming-service
  template:
    metadata:
      labels:
        app: streaming-service
    spec:
      containers:
      - name: api
        image: myregistry.azurecr.io/streaming-service:1.0.0
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        envFrom:
        - configMapRef:
            name: streaming-config
        - secretRef:
            name: streaming-secrets
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

---

## 📝 Notas Finales

### Checklist Pre-Producción

- [ ] Cambiar `Jwt__Secret` a valor seguro de 64+ caracteres
- [ ] Configurar `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Deshabilitar `SignalR__EnableDetailedErrors`
- [ ] Configurar CORS con orígenes específicos (modificar `Program.cs`)
- [ ] Configurar HTTPS con certificados válidos
- [ ] Implementar backplane de SignalR (Redis) para escalabilidad horizontal
- [ ] Configurar logging estructurado (Serilog + Elasticsearch/Application Insights)
- [ ] Implementar rate limiting
- [ ] Configurar CDN real para streaming URLs
- [ ] Establecer backup automático de MongoDB
- [ ] Configurar monitoring y alertas (Prometheus, Grafana, Azure Monitor)

### Recursos Adicionales

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [MongoDB Connection String](https://www.mongodb.com/docs/manual/reference/connection-string/)
- [RabbitMQ Configuration](https://www.rabbitmq.com/configure.html)
- [SignalR Scale-out with Redis](https://learn.microsoft.com/en-us/aspnet/core/signalr/redis-backplane)

---

**Última Actualización**: 2026-01-20  
**Versión**: 1.0
