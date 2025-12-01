# 🛠️ Gestión de Base de Datos - OpenUpTool API

## 📋 Descripción

Se ha agregado una nueva sección en el Swagger llamada **"DatabaseManagement"** que permite gestionar fácilmente la base de datos durante el desarrollo. Esta sección incluye tres endpoints útiles para inicializar y poblar la base de datos sin necesidad de hacerlo manualmente desde el frontend.

## 🌐 Acceso

Una vez que la API esté corriendo, abre tu navegador en:

```
http://localhost:5000
```

En Swagger UI verás una nueva sección llamada **"DatabaseManagement"**.

---

## 🔧 Endpoints Disponibles

### 1. 📊 GET `/api/DatabaseManagement/status`

**Descripción**: Verifica el estado actual de la base de datos y cuenta los registros en cada tabla.

**Cuándo usar**: Antes de ejecutar cualquier operación, para saber si la base de datos está vacía o tiene datos.

**Respuesta exitosa**:

```json
{
    "status": "connected",
    "isEmpty": false,
    "message": "Base de datos con datos",
    "registros": {
        "roles": 4,
        "usuarios": 4,
        "proyectos": 2,
        "fases": 8,
        "iteraciones": 2,
        "userStories": 3,
        "artefactos": 0,
        "notificaciones": 2
    }
}
```

---

### 2. 🔨 POST `/api/DatabaseManagement/recreate-database`

**Descripción**: ⚠️ **ELIMINA COMPLETAMENTE** todos los datos y recrea todas las tablas desde cero.

**⚠️ ADVERTENCIA**: Este endpoint elimina TODOS los datos existentes. Úsalo solo cuando quieras empezar desde cero.

**Cuándo usar**:

-   Cuando quieras limpiar completamente la base de datos
-   Cuando las tablas estén corruptas o desactualizadas
-   Al inicio del desarrollo para asegurar una base de datos limpia

**Respuesta exitosa**:

```json
{
    "message": "Base de datos recreada exitosamente. Todas las tablas han sido creadas.",
    "tablas": [
        "Roles",
        "Users",
        "Projects",
        "Phases",
        "ProjectPlans",
        "Milestones",
        "Iterations",
        "ArtifactTypes",
        "Artifacts",
        "ArtifactVersions",
        "UserStories",
        "IterationScope",
        "ProjectUserRoles",
        "ProjectInvitations",
        "Notifications"
    ]
}
```

---

### 3. 🌱 POST `/api/DatabaseManagement/seed-data`

**Descripción**: Siembra datos de prueba en la base de datos para poder trabajar inmediatamente.

**Pre-requisito**: La base de datos debe estar vacía (ejecuta `recreate-database` primero si tiene datos).

**Cuándo usar**:

-   Después de recrear la base de datos
-   Cuando necesites datos de prueba realistas
-   Para demostrar funcionalidades del sistema

**Datos que crea**:

#### 👥 4 Roles:

-   **Admin** - Administrador del sistema
-   **Manager** - Gerente de proyecto
-   **Developer** - Desarrollador
-   **Tester** - Tester

#### 👤 4 Usuarios (todos con password: `Password123!`):

| Email                    | Nombre                | Rol       |
| ------------------------ | --------------------- | --------- |
| admin@openuptool.com     | Administrador Sistema | Admin     |
| manager@openuptool.com   | Carlos Gerente        | Manager   |
| developer@openuptool.com | Ana Desarrolladora    | Developer |
| tester@openuptool.com    | Luis Tester           | Tester    |

#### 📁 2 Proyectos:

1. **Sistema de Gestión Empresarial (SGE)**

    - Estado: En Progreso
    - Con 4 fases de OpenUP
    - Fase Incepción completada
    - Fase Elaboración en progreso

2. **Aplicación Mobile Banking (AMB)**
    - Estado: Creado
    - Con 4 fases pendientes

#### 📊 Datos adicionales:

-   **8 Fases** (4 por proyecto: Incepción, Elaboración, Construcción, Transición)
-   **2 Planes de Proyecto** con objetivos, alcance y cronograma completo
-   **5 Milestones** distribuidos entre los proyectos
-   **6 Tipos de Artefactos** (Vision Document, Use Case Model, Architecture Notebook, etc.)
-   **4 Artefactos** en diferentes fases y estados de aprobación
-   **8 Versiones de Artefactos** con historial de cambios completo
-   **2 Iteraciones** para el proyecto SGE
-   **3 User Stories** con diferentes estados
-   **2 Items en el alcance** de la iteración actual
-   **2 Notificaciones** de ejemplo

**Respuesta exitosa**:

```json
{
    "message": "Datos de prueba sembrados exitosamente",
    "resumen": {
        "roles": 4,
        "usuarios": 4,
        "proyectos": 2,
        "fases": 8,
        "planesProyecto": 2,
        "milestones": 5,
        "tiposArtefactos": 6,
        "artefactos": 4,
        "versionesArtefactos": 8,
        "iteraciones": 2,
        "userStories": 3,
        "alcanceIteracion": 2,
        "notificaciones": 2
    },
    "credenciales": {
        "password": "Password123!",
        "usuarios": [
            { "email": "admin@openuptool.com", "rol": "Admin" },
            { "email": "manager@openuptool.com", "rol": "Manager" },
            { "email": "developer@openuptool.com", "rol": "Developer" },
            { "email": "tester@openuptool.com", "rol": "Tester" }
        ]
    }
}
```

---

## 🚀 Flujo de Trabajo Recomendado

### Inicialización Completa

**Opción 1: Desde Cero (Base de datos vacía)**

```
1. GET  /api/DatabaseManagement/status          → Verificar estado
2. POST /api/DatabaseManagement/recreate-database → Limpiar y crear tablas
3. POST /api/DatabaseManagement/seed-data        → Poblar con datos de prueba
```

**Opción 2: Si ya tienes la base de datos creada pero vacía**

```
1. GET  /api/DatabaseManagement/status     → Confirmar que está vacía
2. POST /api/DatabaseManagement/seed-data  → Poblar con datos de prueba
```

### Reiniciar Base de Datos

Cuando necesites empezar de nuevo:

```
1. POST /api/DatabaseManagement/recreate-database → Eliminar todo
2. POST /api/DatabaseManagement/seed-data        → Agregar datos nuevos
```

---

## 📝 Cómo Usar en Swagger

1. **Abre Swagger UI**: `http://localhost:5000`

2. **Busca la sección "DatabaseManagement"**: Debería aparecer en la lista de controladores

3. **Expande el endpoint que quieras usar**: Click en el nombre del endpoint

4. **Click en "Try it out"**: Botón en la esquina superior derecha

5. **Click en "Execute"**: Ejecuta la petición

6. **Revisa la respuesta**: Aparecerá abajo con el código de estado y el JSON de respuesta

---

## 🔐 Credenciales de Prueba

Después de ejecutar `seed-data`, puedes iniciar sesión con cualquiera de estos usuarios:

```
Email: admin@openuptool.com
Password: Password123!
Rol: Administrador (acceso total)

Email: manager@openuptool.com
Password: Password123!
Rol: Manager (gestión de proyectos)

Email: developer@openuptool.com
Password: Password123!
Rol: Developer (desarrollo)

Email: tester@openuptool.com
Password: Password123!
Rol: Tester (pruebas)
```

---

## ⚠️ Notas Importantes

1. **Solo para Desarrollo**: Estos endpoints están pensados solo para desarrollo. En producción deberían estar protegidos o deshabilitados.

2. **Sin Autenticación**: Por facilidad, estos endpoints permiten acceso sin autenticación (`[AllowAnonymous]`).

3. **Datos de Prueba Realistas**: Los datos creados son realistas y siguen el flujo de OpenUP para facilitar las pruebas.

4. **Idempotencia**: Si ejecutas `seed-data` en una base de datos que ya tiene datos, recibirás un error. Debes ejecutar `recreate-database` primero.

---

## 🎯 Casos de Uso Comunes

### Caso 1: Primera vez usando el sistema

```bash
# 1. Verificar estado
GET /api/DatabaseManagement/status

# 2. Crear tablas
POST /api/DatabaseManagement/recreate-database

# 3. Agregar datos
POST /api/DatabaseManagement/seed-data

# 4. ¡Listo! Ya puedes hacer login desde el frontend
```

### Caso 2: Resetear todo después de pruebas

```bash
# 1. Eliminar todo y recrear
POST /api/DatabaseManagement/recreate-database

# 2. Agregar datos frescos
POST /api/DatabaseManagement/seed-data
```

### Caso 3: Verificar qué hay en la base de datos

```bash
# Revisar contadores
GET /api/DatabaseManagement/status
```

---

## 🐛 Solución de Problemas

### Error: "La base de datos ya contiene datos"

**Solución**: Ejecuta primero `POST /api/DatabaseManagement/recreate-database`

### Error: "No se puede conectar a la base de datos"

**Solución**:

1. Verifica que PostgreSQL esté corriendo
2. Revisa las credenciales en el archivo `.env`
3. Verifica que la base de datos `openuptool` exista

### La API no muestra la sección DatabaseManagement

**Solución**:

1. Asegúrate de que el archivo `DatabaseManagementController.cs` existe
2. Reinicia la API (`dotnet run`)
3. Refresca la página de Swagger en el navegador

---

## 📊 Estructura de Datos Creados

Los datos creados siguen una estructura lógica:

```
Roles (4)
└── Usuarios (4)
    ├── Admin (1)
    ├── Manager (1)
    ├── Developer (1)
    └── Tester (1)

Proyectos (2)
├── SGE (En Progreso)
│   ├── Fases (4)
│   │   ├── Incepción (Completada)
│   │   ├── Elaboración (En Progreso) ← Iteraciones aquí
│   │   ├── Construcción (Pendiente)
│   │   └── Transición (Pendiente)
│   ├── Team (3 usuarios asignados)
│   ├── Iteraciones (2)
│   │   └── Iteración 1 (En curso)
│   │       └── User Stories (2)
│   └── User Stories (3 totales)
└── AMB (Creado)
    ├── Fases (4, todas pendientes)
    └── Team (1 manager)

Tipos de Artefactos (6)
└── Distribuidos por fase de OpenUP

Notificaciones (2)
└── Ejemplos de asignación y progreso
```

---

## ✅ Checklist de Verificación

Después de ejecutar `seed-data`, deberías poder:

-   ✅ Iniciar sesión con cualquiera de los 4 usuarios
-   ✅ Ver 2 proyectos en el dashboard
-   ✅ Ver las fases de cada proyecto
-   ✅ Ver iteraciones en el proyecto SGE
-   ✅ Ver user stories y su estado
-   ✅ Ver notificaciones en el sistema
-   ✅ Ver usuarios asignados a proyectos

---

**¡Listo!** Ahora puedes trabajar con datos de prueba sin tener que crearlos manualmente. 🎉
