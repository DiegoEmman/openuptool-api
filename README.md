# OpenUpTool API

## 📋 Descripción

**OpenUpTool** es una herramienta de software diseñada para apoyar el desarrollo de sistemas utilizando la metodología **OpenUP** (Open Unified Process). Esta API REST proporciona funcionalidades para gestionar proyectos de desarrollo completos a través de las cuatro fases principales: **Inicio (Inception)**, **Elaboración (Elaboration)**, **Construcción (Construction)** y **Transición (Transition)**.

### Objetivos del Proyecto

- Gestionar proyectos completos siguiendo las fases de OpenUP
- Controlar y versionar artefactos específicos de cada fase
- Gestionar flujos de trabajo, iteraciones y microincrementos
- Proporcionar flexibilidad para personalizar el modelo OpenUP
- Integrar con sistemas de control de versiones y CI/CD
- Mantener auditoría completa de cambios y acciones

---

## 🚀 Características Principales

### Por EPIC (basado en Historias de Usuario)

#### ✅ EPIC 1-2: Gestión General y Planificación

- Crear, modificar y archivar proyectos OpenUP
- Definir y dar seguimiento al plan del proyecto
- Registrar avance por iteraciones con métricas

#### ✅ EPIC 3: Gestión de Artefactos por Fase

- **Incepción**: Documento de Visión, Stakeholders, Riesgos, Plan inicial, Casos de Uso
- **Elaboración**: Modelo de Dominio, Arquitectura, Requerimientos, Prototipos
- **Construcción**: Diseño Detallado, Código, Pruebas, Resultados
- **Transición**: Manuales, Plan de Despliegue, Producto Final, Cierre

#### ✅ EPIC 4: Versionado y Control

- Control de versiones completo para cada artefacto
- Historial de cambios con fecha, autor y observaciones
- Marcado de entregables como obligatorios u opcionales

#### ✅ EPIC 5: Flujos de Trabajo

- Definir flujos personalizados (revisión, QA, aprobación)
- Asignar entregables a flujos
- Control de permisos por rol y estado

#### ✅ EPIC 6: Metodología Ágil

- Gestión de iteraciones (Sprints)
- Registro de microincrementos
- Control de velocidad del equipo
- Registro de defectos vinculados a pruebas

#### ✅ EPIC 7: Flexibilidad

- Redefinir artefactos, flujos, roles y etapas
- Guardar y versionar plantillas OpenUP personalizadas
- Reasignar entregables entre fases

#### ✅ EPIC 8-9: Usabilidad y Seguridad

- Sistema de notificaciones configurable
- Integración con repositorios Git y CI
- Exportación/importación de proyectos
- Auditoría completa de acciones
- Gestión de usuarios y roles

#### ✅ EPIC 10: Cierre de Proyecto

- Validación de requisitos obligatorios
- Generación automática de documento de cierre
- Archivado con preservación de historial

---

## 🛠️ Stack Tecnológico

| Tecnología              | Versión | Propósito            |
| ----------------------- | ------- | -------------------- |
| .NET                    | 9.0     | Framework principal  |
| ASP.NET Core            | 9.0     | Web API REST         |
| Entity Framework Core   | 9.0     | ORM                  |
| PostgreSQL              | 16      | Base de datos        |
| Docker & Docker Compose | -       | Contenedores         |
| Swagger/OpenAPI         | 7.2     | Documentación API    |
| MediatR                 | 12.4    | CQRS Pattern         |
| FluentValidation        | 11.3    | Validaciones         |
| xUnit                   | 2.9     | Testing              |
| DotNetEnv               | 3.1     | Variables de entorno |

---

## 📁 Estructura del Proyecto

```
OpenUpTool/
├── src/
│   ├── OpenUpTool.Api/              # API REST - Capa de Presentación
│   ├── OpenUpTool.Core/             # Dominio - Lógica de negocio
│   └── OpenUpTool.Infrastructure/   # Infraestructura - Acceso a datos
├── tests/
│   └── OpenUpTool.Tests/            # Pruebas
├── docker/
│   └── init-scripts/                # Scripts de inicialización de BD
├── docker-compose.yml               # Orquestación de contenedores
├── .env                            # Variables de entorno (git ignored)
├── .env.example                    # Template de configuración
└── ARCHITECTURE.md                 # Documentación de arquitectura
```

> **📖 Ver [ARCHITECTURE.md](./ARCHITECTURE.md) para detalles de arquitectura y patrones**

---

## 🐳 Inicio Rápido con Docker

### Prerequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 1. Clonar el Repositorio

```bash
git clone https://github.com/DiegoEmman/openuptool-api.git
cd openuptool-api
```

### 2. Configurar Variables de Entorno

```bash
# Copiar el template de configuración
cp .env.example .env

# Editar .env con tus valores (puedes usar los valores por defecto para desarrollo)
# IMPORTANTE: Cambiar JWT_SECRET y DB_PASSWORD en producción
```

### 3. Iniciar la Base de Datos con Docker

```bash
# Iniciar PostgreSQL y PgAdmin
docker-compose up -d

# Verificar que los contenedores estén corriendo
docker-compose ps
```

**Acceso a PgAdmin:**

- URL: http://localhost:5050
- Email: admin@openuptool.com
- Password: admin (configurable en .env)

### 4. Restaurar Dependencias

```bash
dotnet restore
```

### 5. Aplicar Migraciones (cuando estén disponibles)

```bash
cd src/OpenUpTool.Api
dotnet ef database update
```

### 6. Ejecutar la API

```bash
# Desde la raíz del proyecto
dotnet run --project src/OpenUpTool.Api/OpenUpTool.Api.csproj
```

La API estará disponible en:

- **HTTP**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

---

## 🔧 Comandos Útiles

### Desarrollo

```bash
# Compilar la solución
dotnet build

# Ejecutar tests
dotnet test

# Ver logs de Docker
docker-compose logs -f postgres

# Detener contenedores
docker-compose down

# Detener y eliminar volúmenes (⚠️ elimina datos)
docker-compose down -v
```

### Entity Framework

```bash
# Crear una nueva migración
dotnet ef migrations add NombreMigracion --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api

# Aplicar migraciones
dotnet ef database update --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api

# Revertir última migración
dotnet ef migrations remove --project src/OpenUpTool.Infrastructure --startup-project src/OpenUpTool.Api
```

---

## 🔐 Configuración de Seguridad

### JWT Authentication

El proyecto usa JWT Bearer Tokens para autenticación. Configura las variables en `.env`:

```env
JWT_SECRET=your-super-secret-key-minimum-32-characters
JWT_ISSUER=OpenUpTool
JWT_AUDIENCE=OpenUpToolUsers
JWT_EXPIRATION_MINUTES=60
JWT_REFRESH_EXPIRATION_DAYS=7
```

### CORS

Configura los orígenes permitidos en `.env`:

```env
CORS_ORIGINS=http://localhost:3000,http://localhost:4200
```

---

## 📊 Base de Datos

### Conexión a PostgreSQL

**Desde aplicación:**

```
Host=localhost
Port=5432
Database=openuptool
Username=openuptool_user
Password=DevPassword123!
```

**Desde PgAdmin:**

1. Acceder a http://localhost:5050
2. Add New Server
3. General > Name: OpenUpTool
4. Connection:
   - Host: postgres (nombre del servicio Docker)
   - Port: 5432
   - Username: openuptool_user
   - Password: DevPassword123!

### Extensiones Instaladas

- **uuid-ossp**: Generación de UUIDs
- **pgcrypto**: Funciones criptográficas

---

## 📚 Documentación API

Una vez iniciada la aplicación, la documentación interactiva de Swagger estará disponible en:

🔗 **http://localhost:5000/swagger**

Swagger proporciona:

- Exploración de todos los endpoints
- Esquemas de request/response
- Capacidad de probar endpoints directamente
- Autenticación JWT integrada

---

## 🧪 Testing

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ejecutar tests de un proyecto específico
dotnet test tests/OpenUpTool.Tests/OpenUpTool.Tests.csproj
```

---

## 📦 Deployment

### Docker

```bash
# Construir imagen
docker build -t openuptool-api:latest .

# Ejecutar contenedor
docker run -d -p 5000:80 --env-file .env openuptool-api:latest
```

### Con Docker Compose (incluye BD)

```bash
# Producción
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## 🤝 Metodologías Aplicadas

### Scrum

- **Product Backlog**: 26 Historias de Usuario definidas
- **Sprints/Iteraciones**: Módulo de gestión implementado
- **Estimación**: Story points por HU
- **Velocidad**: Control de velocidad del equipo

### XP (Extreme Programming)

- **TDD**: Estructura de tests preparada
- **Continuous Integration**: Ready para CI/CD
- **Simple Design**: YAGNI y SOLID aplicados
- **Refactoring**: Código limpio y mantenible
- **Pair Programming**: Propiciado con Git flow

---

## 📋 Historias de Usuario

El proyecto implementa **26 Historias de Usuario** distribuidas en **10 EPICs**:

1. **EPIC 1**: Gestión general del proyecto OpenUP (2 HUs)
2. **EPIC 2**: Plan del proyecto y seguimiento (3 HUs)
3. **EPIC 3**: Gestión de artefactos por fase (4 HUs)
4. **EPIC 4**: Versionado y control de entregables (2 HUs)
5. **EPIC 5**: Flujos de trabajo y trazabilidad (3 HUs)
6. **EPIC 6**: Iteraciones y microincrementos (3 HUs)
7. **EPIC 7**: Flexibilidad y personalización (3 HUs)
8. **EPIC 8**: Usabilidad y notificaciones (3 HUs)
9. **EPIC 9**: Calidad y auditoría (2 HUs)
10. **EPIC 10**: Cierre de proyecto (1 HU)

---

## 🔄 Estado del Proyecto

**Versión Actual**: 1.0.0-alpha

### ✅ Completado

- [x] Estructura base del proyecto
- [x] Configuración de Docker y PostgreSQL
- [x] Sistema de variables de entorno
- [x] Configuración de Swagger
- [x] Arquitectura en capas (Clean Architecture)
- [x] Preparación para JWT

### 🚧 En Desarrollo

- [ ] Definición de entidades del dominio
- [ ] Migraciones de EF Core
- [ ] Implementación de repositorios
- [ ] Controllers por EPIC
- [ ] Autenticación y autorización
- [ ] Validaciones con FluentValidation

### 📅 Pendiente

- [ ] Tests unitarios y de integración
- [ ] Sistema de notificaciones
- [ ] Integración con Git
- [ ] Export/Import de proyectos
- [ ] CI/CD pipeline

---

## 🐛 Troubleshooting

### Error de conexión a PostgreSQL

```bash
# Verificar que el contenedor está corriendo
docker-compose ps

# Ver logs del contenedor
docker-compose logs postgres

# Reiniciar contenedor
docker-compose restart postgres
```

### Puerto ya en uso

```bash
# Cambiar el puerto en .env
API_PORT=5001
DB_PORT=5433

# O detener el proceso que usa el puerto
# Windows:
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Error en migraciones

```bash
# Eliminar base de datos y volver a crear
docker-compose down -v
docker-compose up -d
dotnet ef database update
```

---

## 📞 Contacto y Soporte

- **Repositorio**: [github.com/DiegoEmman/openuptool-api](https://github.com/DiegoEmman/openuptool-api)
- **Issues**: [Reportar bug o sugerencia](https://github.com/DiegoEmman/openuptool-api/issues)

---

## 📄 Licencia

[Especificar licencia según corresponda]

---

## 👥 Contribuciones

Este proyecto se desarrolla siguiendo metodologías ágiles (Scrum + XP). Para contribuir:

1. Fork del repositorio
2. Crear rama feature (`git checkout -b feature/AmazingFeature`)
3. Commit de cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir Pull Request

---

**Desarrollado con ❤️ aplicando Scrum y XP**
