# Streaming Service

Microservicio de streaming en vivo para gestionar sesiones de transmisión, control de acceso, capacidad de espectadores y comunicación en tiempo real para eventos.

## 📋 Tabla de Contenidos

- [Documentación Técnica](#-documentación-técnica)
- [¿Qué Problema Resuelve?](#-qué-problema-resuelve)
- [Stack Tecnológico](#-stack-tecnológico)
- [Quick Start](#-quick-start)
- [Estado del Proyecto](#-estado-del-proyecto)

## 📖 Documentación Técnica

- **[docs/architecture.md](docs/architecture.md)** - Arquitectura interna, flujo de datos y dependencias
- **[docs/api.md](docs/api.md)** - Documentación de endpoints REST y SignalR
- **[docs/setup.md](docs/setup.md)** - Configuración detallada, variables de entorno y despliegue
- **[docs/guia-consumo-frontend.md](docs/guia-consumo-frontend.md)** - Guía de integración para clientes frontend

## 🎯 ¿Qué Problema Resuelve?

Este servicio gestiona el **acceso controlado a transmisiones de video en vivo** para eventos, resolviendo:

1. **Control de Capacidad**: Limita el número de espectadores simultáneos por sesión
2. **Gestión de Acceso**: Genera y valida tokens de acceso basados en reservaciones
3. **Tiempo Real**: Proporciona actualizaciones instantáneas de contadores de espectadores y chat mediante SignalR
4. **Prevención de Abuso**: Control de reconexiones (máx. 3 por hora) y validación de tokens JWT
5. **Simulación de Streaming**: Endpoints mock para HLS/DASH que simulan un CDN de streaming

### Casos de Uso Principales

- Crear sesiones de streaming programadas para eventos
- Generar tokens de acceso para usuarios con reservaciones confirmadas
- Validar acceso y proporcionar URLs de streaming cifradas
- Monitorear en tiempo real el número de espectadores
- Gestionar colas de espera cuando se alcanza la capacidad máxima
- Registrar auditoría de accesos y eventos del sistema

## 🛠 Stack Tecnológico

### Backend
- **.NET 8.0** - Framework principal
- **ASP.NET Core** - API REST y hosting
- **MediatR** - Patrón CQRS y mediación de comandos/queries
- **SignalR** - Comunicación en tiempo real (WebSockets)

### Persistencia y Mensajería
- **MongoDB 7.0** - Base de datos NoSQL para sesiones, accesos y auditoría
- **RabbitMQ 3.12** - Message broker para eventos inter-microservicios

### Seguridad
- **JWT Bearer Authentication** - Autenticación basada en tokens
- **System.IdentityModel.Tokens.Jwt** - Generación y validación de tokens

### Arquitectura
- **Hexagonal (Ports & Adapters)** - Separación clara de capas
- **Domain-Driven Design** - Value Objects, Entities, Domain Events

## 🚀 Quick Start

### Prerequisitos
- Docker y Docker Compose
- .NET 8.0 SDK (opcional, para desarrollo local)

### Levantar el servicio con Docker Compose

```bash
# Clonar el repositorio
git clone https://github.com/eventmesh-lab/streaming-service.git
cd streaming-service

# Copiar variables de entorno
cp .env.docker.example .env

# Iniciar servicios (API, MongoDB, RabbitMQ)
docker-compose up -d

# Verificar que el servicio está activo
curl http://localhost:7001/health
```

### Acceso a Interfaces

- **API**: http://localhost:7001
- **Swagger UI**: http://localhost:7001/swagger
- **RabbitMQ Management**: http://localhost:15673 (usuario: `streaming_user`, password: `streaming_pass_2024`)
- **MongoDB**: `localhost:27020` (usuario: `admin`, password: `streaming_pass_2024`)

### Desarrollo Local (sin Docker)

```bash
# Asegurarse de tener MongoDB y RabbitMQ ejecutándose localmente
# o modificar appsettings.Development.json con las conexiones apropiadas

# Restaurar dependencias
dotnet restore

# Ejecutar la API
cd src/streaming-service.Api
dotnet run

# Ejecutar tests
dotnet test
```

## 📊 Estado del Proyecto

### Características Implementadas ✅
- Gestión de sesiones de streaming (CRUD)
- Generación y validación de tokens de acceso
- Refresh de tokens
- Control de capacidad y colas de espera
- SignalR Hub para tiempo real (contadores, chat)
- Endpoints simulados de HLS/DASH
- Integración con MongoDB
- Publicación de eventos a RabbitMQ
- Auditoría de accesos

### En Desarrollo / Simulado ⚠️
- Integración con CDN real (actualmente simulado)
- Procesamiento de grabaciones (stub)
- Procesamiento de imágenes (stub)
- Cifrado de URLs de streaming (parcialmente implementado)

---

**Versión**: 1.0.0  
**Licencia**: MIT  
**Contacto**: EventMesh Team

