# OpenUpTool - Guía de Configuración y Ejecución

## Descripción

OpenUpTool es una aplicación completa para la gestión de proyectos usando la metodología OpenUP, con un backend en ASP.NET Core y un frontend en React.

## Arquitectura

-   **Backend**: ASP.NET Core 9.0 con Entity Framework Core y PostgreSQL
-   **Frontend**: React 18 con TypeScript y Material-UI
-   **Base de Datos**: PostgreSQL 16

## Requisitos Previos

-   .NET 9.0 SDK
-   Node.js 18+ y npm/pnpm
-   Docker y Docker Compose (para la base de datos)

---

## Configuración del Backend

### 1. Iniciar la Base de Datos con Docker

```bash
cd openuptool-api
docker-compose up -d
```

Esto levantará:

-   PostgreSQL en el puerto 5432
-   PgAdmin en http://localhost:5050 (usuario: admin@openuptool.com, password: admin)

### 2. Configurar Variables de Entorno

Copia el archivo `.env.example` a `.env` y ajusta los valores si es necesario:

```bash
cp .env.example .env
```

Variables principales:

-   `DB_HOST=localhost`
-   `DB_PORT=5432`
-   `DB_NAME=openuptool`
-   `DB_USER=openuptool_user`
-   `DB_PASSWORD=dev_password_123`
-   `CORS_ORIGINS=http://localhost:5173,http://localhost:3000`

### 3. Restaurar Paquetes NuGet

```bash
dotnet restore
```

### 4. Aplicar Migraciones (opcional, se aplican automáticamente al iniciar)

```bash
cd src/OpenUpTool.Api
dotnet ef database update
```

### 5. Ejecutar el Backend

```bash
cd src/OpenUpTool.Api
dotnet run
```

El backend estará disponible en:

-   API: http://localhost:5000
-   Swagger: http://localhost:5000 (raíz)

---

## Configuración del Frontend

### 1. Instalar Dependencias

```bash
cd openuptool-front
npm install
# o
pnpm install
```

### 2. Configurar Variables de Entorno

Copia el archivo `.env.example` a `.env`:

```bash
cp .env.example .env
```

Variables:

-   `VITE_API_URL=http://localhost:5000/api`

### 3. Ejecutar el Frontend

```bash
npm run dev
# o
pnpm dev
```

El frontend estará disponible en: http://localhost:5173

---

## Endpoints de la API

### Proyectos

-   `GET /api/projects` - Listar todos los proyectos
-   `GET /api/projects/{id}` - Obtener un proyecto
-   `POST /api/projects` - Crear proyecto
-   `PATCH /api/projects/{id}` - Actualizar proyecto
-   `DELETE /api/projects/{id}` - Eliminar proyecto

### Fases

-   `GET /api/projects/{projectId}/phases` - Listar fases del proyecto
-   `GET /api/projects/{projectId}/phases/{phaseCode}` - Obtener fase por código
-   `PATCH /api/projects/{projectId}/phases/{phaseId}` - Actualizar fase
-   `POST /api/projects/{projectId}/phases/{phaseId}/start` - Iniciar fase
-   `POST /api/projects/{projectId}/phases/{phaseId}/complete` - Completar fase

### Planes

-   `GET /api/projects/{projectId}/plan` - Obtener plan del proyecto
-   `POST /api/projects/{projectId}/plan` - Crear plan inicial

### Iteraciones

-   `GET /api/projects/{projectId}/iterations` - Listar iteraciones
-   `POST /api/projects/{projectId}/iterations` - Crear iteración
-   `PATCH /api/projects/{projectId}/iterations/{iterationId}/status` - Actualizar estado

### Artefactos

-   `GET /api/projects/{projectId}/artifacts?phaseId={phaseId}` - Listar artefactos
-   `POST /api/projects/{projectId}/artifacts` - Crear artefacto
-   `PATCH /api/projects/{projectId}/artifacts/{artifactId}` - Actualizar artefacto

### Tipos de Artefactos

-   `GET /api/artifact-types` - Listar todos los tipos
-   `GET /api/artifact-types?phase={phaseCode}` - Listar tipos por fase
-   `POST /api/artifact-types/seed-inception` - Inicializar tipos de Inception

---

## Estructura del Proyecto Backend

```
openuptool-api/
├── src/
│   ├── OpenUpTool.Core/           # Dominio y lógica de negocio
│   │   ├── Entities/              # Entidades del dominio
│   │   ├── DTOs/                  # Objetos de transferencia de datos
│   │   ├── Interfaces/            # Interfaces de repositorios y servicios
│   │   └── Services/              # Implementación de servicios
│   ├── OpenUpTool.Infrastructure/ # Acceso a datos
│   │   ├── Data/                  # DbContext y configuraciones
│   │   └── Repositories/          # Implementación de repositorios
│   └── OpenUpTool.Api/            # API REST
│       └── Controllers/           # Controladores HTTP
├── docker/
│   └── init-scripts/
│       └── 01-init.sql           # Script de inicialización de BD
├── docker-compose.yml            # Configuración de Docker
└── .env                          # Variables de entorno
```

---

## Estructura del Proyecto Frontend

```
openuptool-front/
├── src/
│   ├── pages/                    # Páginas principales
│   │   └── Projects/             # Páginas de proyectos
│   ├── components/               # Componentes reutilizables
│   │   ├── artifacts/            # Componentes de artefactos
│   │   ├── iterations/           # Componentes de iteraciones
│   │   └── plan/                 # Componentes del plan
│   ├── services/                 # Servicios HTTP
│   │   └── api/                  # Cliente HTTP
│   ├── types/                    # Definiciones de tipos TypeScript
│   ├── config/                   # Configuración
│   └── styles/                   # Estilos globales
└── .env                          # Variables de entorno
```

---

## Solución de Problemas

### Error de conexión a la base de datos

-   Verifica que Docker esté corriendo: `docker ps`
-   Reinicia los contenedores: `docker-compose restart`
-   Revisa los logs: `docker-compose logs postgres`

### Error CORS en el frontend

-   Verifica que `CORS_ORIGINS` en el backend incluya la URL del frontend
-   Asegúrate de que el backend esté corriendo

### Swagger no está disponible

-   Solo está habilitado en modo Development
-   Verifica `ASPNETCORE_ENVIRONMENT=Development` en el `.env`

---

## Comandos Útiles

### Backend

```bash
# Crear migración
dotnet ef migrations add MigrationName -p src/OpenUpTool.Infrastructure -s src/OpenUpTool.Api

# Eliminar última migración
dotnet ef migrations remove -p src/OpenUpTool.Infrastructure -s src/OpenUpTool.Api

# Ver base de datos
docker exec -it openuptool-db psql -U openuptool_user -d openuptool
```

### Docker

```bash
# Ver logs de la base de datos
docker-compose logs -f postgres

# Detener servicios
docker-compose down

# Detener y eliminar volúmenes (¡cuidado, elimina datos!)
docker-compose down -v
```

---

## Licencia

Este proyecto es de código abierto bajo licencia MIT.
