# 📖 Manual de Usuario - OpenUpTool

## Tabla de Contenidos

1. [Introducción](#introducción)
2. [Acceso al Sistema](#acceso-al-sistema)
3. [Panel Principal](#panel-principal)
4. [Gestión de Proyectos](#gestión-de-proyectos)
5. [Gestión de Fases](#gestión-de-fases)
6. [Gestión de Iteraciones](#gestión-de-iteraciones)
7. [Gestión de Artefactos](#gestión-de-artefactos)
8. [Flujos de Trabajo](#flujos-de-trabajo)
9. [Pruebas y Defectos](#pruebas-y-defectos)
10. [Configuración Global](#configuración-global)
11. [Notificaciones](#notificaciones)
12. [Invitaciones y Colaboración](#invitaciones-y-colaboración)
13. [Preguntas Frecuentes](#preguntas-frecuentes)

---

## Introducción

**OpenUpTool** es una herramienta completa para la gestión de proyectos de desarrollo de software siguiendo la metodología **OpenUP** (Open Unified Process). La interfaz web permite gestionar todas las fases del proyecto, desde la concepción hasta el cierre, con seguimiento completo de artefactos, iteraciones y equipos de trabajo.

### Características Principales

- ✅ Gestión completa de proyectos OpenUP
- 📊 Seguimiento de las 4 fases: Inicio, Elaboración, Construcción y Transición
- 📝 Control de artefactos y versionado
- 🔄 Gestión de iteraciones (sprints) y microincrementos
- 🧪 Registro y seguimiento de pruebas y defectos
- 👥 Colaboración en equipo con roles y permisos
- 🔔 Sistema de notificaciones
- 📈 Métricas y gráficos de progreso

---

## Acceso al Sistema

### Inicio de Sesión

1. Accede a la URL de la aplicación en tu navegador
2. Verás la **página de inicio de sesión** con un formulario centrado
3. Ingresa tus credenciales:
   - **Email**: Tu dirección de correo electrónico
   - **Contraseña**: Tu contraseña personal
4. Haz clic en **"Iniciar Sesión"**

**Nota**: Si olvidaste tu contraseña, contacta al administrador del sistema.

### Roles de Usuario

El sistema maneja diferentes roles con distintos permisos:

- **Admin**: Acceso completo al sistema
- **Manager**: Gestión de proyectos y equipos
- **Developer**: Desarrollo y creación de artefactos
- **Tester**: Ejecución de pruebas y registro de defectos
- **Viewer**: Solo lectura

---

## Panel Principal

Al iniciar sesión, llegarás al **Dashboard principal** que muestra:

### Sección de Bienvenida

- Mensaje de bienvenida personalizado
- Resumen de tus proyectos activos
- Acceso rápido a notificaciones pendientes

### Navegación Principal

La barra de navegación superior incluye:

- **🏠 Inicio**: Volver al dashboard
- **📁 Proyectos**: Lista de todos tus proyectos
- **🔔 Notificaciones**: Alertas y mensajes del sistema
- **📨 Invitaciones**: Invitaciones pendientes a proyectos
- **⚙️ Configuración**: Configuración global (solo Admin/Manager)
- **👤 Perfil**: Información de tu cuenta y cerrar sesión

---

## Gestión de Proyectos

### Listar Proyectos

1. Haz clic en **"Proyectos"** en la barra de navegación
2. Verás una tabla con todos tus proyectos activos:
   - Nombre del proyecto
   - Identificador único
   - Estado (Creado, Planificado, En curso, Cerrado)
   - Fecha de inicio
   - Propietario
   - Acciones disponibles

### Pestañas de Proyectos

- **Proyectos Activos**: Proyectos en los que estás trabajando
- **Proyectos Archivados**: Proyectos finalizados o archivados

### Crear un Nuevo Proyecto

1. En la página de proyectos, haz clic en **"+ Nuevo Proyecto"**
2. Completa el formulario:
   - **Nombre**: Nombre descriptivo del proyecto
   - **Identificador**: Código único (ej: PROJ-001)
   - **Descripción**: Descripción detallada del proyecto
   - **Fecha de Inicio**: Fecha de inicio planificada
   - **Propietario**: Responsable principal
   - **Tags**: Etiquetas para organización (opcional)
3. Haz clic en **"Crear Proyecto"**

**Nota**: Solo usuarios con rol Admin o Manager pueden crear proyectos.

### Ver Detalles de un Proyecto

1. Haz clic en el nombre del proyecto o en el botón **"Ver"**
2. Se mostrará el **panel de detalles del proyecto** con:
   - Información general
   - Estado actual
   - Plan del proyecto
   - Acceso a todas las fases
   - Métricas y progreso

### Editar un Proyecto

1. En la vista de detalles del proyecto, haz clic en **"Editar Proyecto"**
2. Modifica los campos necesarios
3. Haz clic en **"Guardar Cambios"**

### Archivar un Proyecto

1. En la lista de proyectos, localiza el proyecto a archivar
2. Haz clic en el botón de **menú de acciones** (⋮)
3. Selecciona **"Archivar"**
4. Confirma la acción

**Nota**: Los proyectos archivados se moverán a la pestaña "Proyectos Archivados" pero no se eliminan.

### Plan del Proyecto

Cada proyecto tiene un **Plan** asociado que incluye:

- **Objetivos**: Metas principales del proyecto
- **Alcance**: Definición del alcance
- **Equipo**: Miembros del equipo y sus roles
- **Cronograma**: Fechas clave e hitos
- **Recursos**: Recursos asignados
- **Riesgos**: Identificación de riesgos

Para gestionar el plan:

1. Ve a la vista de detalles del proyecto
2. Haz clic en la pestaña **"Plan"**
3. Edita las secciones necesarias
4. Guarda los cambios

---

## Gestión de Fases

OpenUP define 4 fases principales. Cada fase tiene sus propios artefactos y objetivos.

### Fase de Inicio (Inception)

**Objetivo**: Establecer la visión y el alcance del proyecto.

Para acceder:

1. Ve a la vista de detalles del proyecto
2. La información de inicio se encuentra en el **Dashboard del proyecto**

**Artefactos típicos**:

- Documento de Visión
- Lista de Stakeholders
- Identificación de Riesgos
- Plan inicial
- Casos de Uso de alto nivel

### Fase de Elaboración

**Objetivo**: Definir la arquitectura y los requisitos detallados.

Para acceder:

1. En el proyecto, haz clic en **"Elaboración"** en el menú lateral
2. Verás la **vista de Fase de Elaboración**

**Funcionalidades**:

- Lista de artefactos de elaboración
- Crear nuevos artefactos
- Validar completitud de la fase
- Ver historial de versiones

**Artefactos típicos**:

- Modelo de Dominio
- Arquitectura del Sistema
- Especificación de Requisitos
- Prototipos
- Casos de Uso detallados

#### Crear Artefacto en Elaboración

1. En la vista de Elaboración, haz clic en **"+ Crear Artefacto"**
2. Completa el formulario:
   - **Tipo de Artefacto**: Selecciona de la lista (Modelo de Dominio, Arquitectura, etc.)
   - **Nombre**: Nombre descriptivo
   - **Descripción**: Descripción detallada
   - **Obligatorio**: Marca si es obligatorio para cerrar la fase
   - **Archivo**: Adjunta el archivo del artefacto
3. Haz clic en **"Crear"**

#### Validar Fase de Elaboración

1. Haz clic en **"Validar Fase"**
2. El sistema verificará:
   - Todos los artefactos obligatorios están completos
   - Artefactos tienen al menos una versión aprobada
3. Se mostrará un diálogo con los resultados:
   - ✅ Artefactos completos
   - ⚠️ Artefactos faltantes
   - ℹ️ Recomendaciones

### Fase de Construcción

**Objetivo**: Implementar y desarrollar el sistema.

Para acceder:

1. En el proyecto, haz clic en **"Construcción"** en el menú lateral

**Funcionalidades**:

- Gestión de artefactos de construcción
- Registro de builds y compilaciones
- Seguimiento de desarrollo
- Integración con repositorios

**Artefactos típicos**:

- Diseño Detallado
- Código Fuente
- Pruebas Unitarias
- Documentación técnica
- Builds del sistema

#### Vista de Construcción

La vista de construcción incluye secciones para:

- **Artefactos de Diseño**: Diagramas UML, diseño de base de datos
- **Código y Componentes**: Enlaces a repositorios
- **Builds**: Registro de compilaciones exitosas
- **Pruebas**: Resultados de pruebas unitarias

### Fase de Transición

**Objetivo**: Desplegar el sistema en producción.

Para acceder:

1. En el proyecto, haz clic en **"Transición"** en el menú lateral

**Artefactos típicos**:

- Manuales de Usuario
- Plan de Despliegue
- Producto Final
- Documentación de Cierre
- Capacitación

---

## Gestión de Iteraciones

Las iteraciones son períodos de tiempo (sprints) en los que se desarrolla un incremento del producto.

### Ver Iteraciones

1. En el proyecto, haz clic en **"Iteraciones"** en el menú lateral
2. Verás la lista de todas las iteraciones:
   - Nombre y número
   - Fechas de inicio y fin
   - Estado (Planificada, En Progreso, Completada)
   - Progreso (%)

### Crear una Iteración

1. Haz clic en **"+ Nueva Iteración"**
2. Completa el formulario:
   - **Nombre**: Nombre de la iteración (ej: Sprint 1)
   - **Número**: Número de iteración
   - **Fecha de Inicio**: Fecha de inicio
   - **Fecha de Fin**: Fecha de cierre
   - **Objetivos**: Objetivos de la iteración
3. Haz clic en **"Crear"**

### Seguimiento de Iteración

Para hacer seguimiento detallado de una iteración:

1. Haz clic en una iteración de la lista
2. Se abrirá la **vista de seguimiento de iteración** con 3 pestañas:

#### Pestaña 1: Progreso

Muestra el panel de progreso con:

- **Resumen de la Iteración**:
  - Fechas
  - Días restantes
  - Progreso general (%)
  - Velocidad del equipo
  - Horas trabajadas
  - Horas estimadas totales
- **Registrar Nuevo Avance**:
  - Fecha del registro
  - Horas trabajadas
  - Porcentaje completado
  - Observaciones
  - Haz clic en **"Registrar Avance"**

#### Pestaña 2: Tareas

Gestiona las tareas de la iteración:

**Crear Tarea**:

1. Haz clic en **"+ Nueva Tarea"**
2. Completa:
   - Título
   - Descripción
   - Estado (Por Hacer, En Progreso, Completada)
   - Prioridad (Baja, Media, Alta)
   - Asignado a
   - Estimación en horas
3. Guarda la tarea

**Actualizar Tarea**:

- Cambia el estado arrastrando la tarea entre columnas
- Edita los detalles haciendo clic en la tarea
- Registra horas trabajadas

#### Pestaña 3: Gráfico Burndown

Visualiza el progreso de la iteración:

- **Gráfico Burndown**: Muestra las horas pendientes vs. días transcurridos
- **Línea ideal**: Representa el progreso ideal
- **Línea real**: Representa el progreso actual
- **Interpretación**:
  - Si la línea real está por debajo de la ideal: vas adelantado ✅
  - Si está por encima: vas retrasado ⚠️

### Microincrementos

Los microincrementos son pequeños avances técnicos dentro de una iteración:

1. En la vista de iteración, sección **"Microincrementos"**
2. Haz clic en **"+ Registrar Microincremento"**
3. Completa:
   - Descripción técnica
   - Tipo (Feature, Bug Fix, Refactor, etc.)
   - Fecha de completado
   - Commit/PR asociado
4. Guarda el microincremento

---

## Gestión de Artefactos

Los artefactos son los entregables del proyecto en cada fase.

### Tipos de Artefactos

Cada fase tiene sus propios tipos de artefactos predefinidos, pero puedes personalizar el catálogo.

### Crear un Artefacto

1. Ve a la fase correspondiente (Elaboración, Construcción, etc.)
2. Haz clic en **"+ Crear Artefacto"**
3. Completa el formulario:
   - **Tipo**: Selecciona el tipo de artefacto
   - **Nombre**: Nombre descriptivo
   - **Descripción**: Detalles del artefacto
   - **Obligatorio**: Marca si es obligatorio
   - **Estado Inicial**: Borrador, En Revisión, Aprobado
   - **Archivo**: Adjunta el documento o archivo
4. Haz clic en **"Crear"**

### Ver Artefactos

La vista de artefactos muestra una tabla con:

- Nombre y tipo
- Estado actual
- Versión
- Flujo de trabajo asociado
- Fecha de última modificación
- Acciones (Ver, Editar, Versionar, Eliminar)

### Versionado de Artefactos

OpenUpTool mantiene un historial completo de versiones:

**Crear una Nueva Versión**:

1. En la lista de artefactos, haz clic en **"Nueva Versión"** en el artefacto deseado
2. Completa:
   - **Número de Versión**: Se autoincrementa (ej: v1.1)
   - **Comentarios**: Descripción de los cambios
   - **Archivo**: Nuevo archivo con los cambios
3. Haz clic en **"Crear Versión"**

**Ver Historial de Versiones**:

1. Haz clic en **"Versiones"** en el artefacto
2. Verás una lista con todas las versiones:
   - Número de versión
   - Fecha de creación
   - Autor
   - Comentarios
   - Acción: Descargar

### Adjuntar Archivos

Los artefactos soportan múltiples tipos de archivos:

- Documentos: PDF, DOCX, MD
- Imágenes: PNG, JPG, SVG
- Diagramas: XML, JSON
- Código: ZIP, TAR.GZ

**Límite de tamaño**: 50MB por archivo

### Eliminar un Artefacto

1. En la lista de artefactos, haz clic en **"Eliminar"** (🗑️)
2. Confirma la acción
3. El artefacto se eliminará permanentemente

**Nota**: No se pueden eliminar artefactos con versiones aprobadas.

---

## Flujos de Trabajo

Los flujos de trabajo definen el ciclo de vida de los artefactos con estados y transiciones.

### Ver Flujos de Trabajo

1. En el proyecto, haz clic en **"Flujos de Trabajo"**
2. Verás tres pestañas:
   - **Lista de Flujos**: Todos los flujos del proyecto
   - **Gestionar Estados**: Estados y transiciones
   - **Permisos**: Matriz de permisos por rol

### Crear un Flujo de Trabajo

1. En la pestaña **"Lista de Flujos"**, haz clic en **"+ Nuevo Flujo"**
2. Completa:
   - **Nombre**: Nombre del flujo (ej: Revisión de Artefactos)
   - **Descripción**: Propósito del flujo
   - **Tipo**: Tipo de artefactos que usarán este flujo
3. Haz clic en **"Crear"**

### Gestionar Estados del Flujo

Los estados representan las etapas del ciclo de vida:

**Crear Estado**:

1. Selecciona un flujo de la lista
2. Ve a la pestaña **"Gestionar Estados"**
3. Haz clic en **"+ Agregar Estado"**
4. Completa:
   - **Nombre**: Nombre del estado (ej: En Revisión)
   - **Descripción**: Descripción
   - **Orden**: Posición en el flujo
   - **Es Estado Final**: Marca si es un estado terminal
   - **Color**: Color visual para identificación
   - **Responsables**: Roles que pueden gestionar este estado
5. Guarda el estado

**Estados Típicos**:

- 📝 Borrador
- 🔄 En Revisión
- ✅ Aprobado
- ❌ Rechazado
- 📦 Archivado

### Transiciones entre Estados

Las transiciones definen cómo un artefacto pasa de un estado a otro:

1. En la vista de estados, define el **orden** de los estados
2. Las transiciones se crean automáticamente según el orden
3. Puedes añadir transiciones personalizadas:
   - Estado origen
   - Estado destino
   - Roles autorizados
   - Condiciones (opcional)

### Matriz de Permisos

Define qué roles pueden realizar acciones en cada estado:

1. Ve a la pestaña **"Permisos"**
2. Verás una matriz de Rol x Estado
3. Para cada celda, marca los permisos:
   - ✏️ **Editar**: Puede modificar el artefacto
   - 🔄 **Cambiar Estado**: Puede mover a otro estado
   - 👁️ **Ver**: Puede visualizar el artefacto
   - 🗑️ **Eliminar**: Puede eliminar el artefacto
4. Los cambios se guardan automáticamente

**Ejemplo de Matriz**:

| Estado      | Admin    | Manager | Developer | Tester | Viewer |
| ----------- | -------- | ------- | --------- | ------ | ------ |
| Borrador    | ✅✅✅✅ | ✅✅✅  | ✅✅      | 👁️     | 👁️     |
| En Revisión | ✅✅✅✅ | ✅✅👁️  | 👁️        | ✅👁️   | 👁️     |
| Aprobado    | ✅✅✅✅ | 👁️      | 👁️        | 👁️     | 👁️     |

### Asignar Flujo a Artefactos

1. Al crear o editar un artefacto
2. En el campo **"Flujo de Trabajo"**, selecciona el flujo deseado
3. El artefacto seguirá los estados y transiciones definidos

---

## Pruebas y Defectos

OpenUpTool incluye un módulo completo para gestión de pruebas y registro de defectos.

### Acceder al Módulo de Pruebas

1. En el proyecto, haz clic en **"Pruebas"** en el menú lateral
2. Verás dos pestañas:
   - **Ejecuciones de Prueba**
   - **Defectos**

### Ejecuciones de Prueba

#### Registrar una Ejecución de Prueba

1. En la pestaña **"Ejecuciones de Prueba"**, haz clic en **"+ Nueva Ejecución"**
2. Completa el formulario:
   - **Nombre del Caso de Prueba**: Identificador de la prueba
   - **Descripción**: Qué se está probando
   - **Tipo**: Unitaria, Integración, Sistema, Aceptación
   - **Fecha de Ejecución**: Fecha en que se ejecutó
   - **Resultado**: Exitosa ✅ / Fallida ❌
   - **Observaciones**: Detalles adicionales
   - **Evidencia**: Adjuntar capturas o logs
3. Haz clic en **"Registrar Ejecución"**

#### Ver Estadísticas de Pruebas

En la parte superior de la pestaña verás:

- **Total de Ejecuciones**: Número total de pruebas ejecutadas
- **Exitosas**: Pruebas que pasaron (%)
- **Fallidas**: Pruebas que fallaron (%)
- **Tasa de Éxito**: Porcentaje de éxito

### Gestión de Defectos

#### Reportar un Defecto

1. Ve a la pestaña **"Defectos"**
2. Haz clic en **"+ Reportar Defecto"**
3. Completa:
   - **Título**: Resumen breve del defecto
   - **Descripción**: Descripción detallada
   - **Severidad**: Crítica, Alta, Media, Baja
   - **Prioridad**: Urgente, Alta, Media, Baja
   - **Estado**: Nuevo, En Progreso, Resuelto, Cerrado
   - **Tipo**: Bug, Error, Mejora, Tarea Técnica
   - **Pasos para Reproducir**: Cómo reproducir el defecto
   - **Resultado Esperado**: Qué debería ocurrir
   - **Resultado Actual**: Qué está ocurriendo
   - **Asignado a**: Desarrollador responsable
   - **Ejecución de Prueba**: Vincular con una prueba (opcional)
   - **Evidencia**: Adjuntar capturas de pantalla o logs
4. Haz clic en **"Reportar"**

#### Gestionar Defectos

En la lista de defectos puedes:

- **Filtrar** por estado, severidad, prioridad, asignado
- **Ordenar** por fecha, severidad, prioridad
- **Ver detalles** haciendo clic en un defecto
- **Editar** el estado, asignación o descripción
- **Cerrar** defectos resueltos

#### Estados de Defectos

- 🆕 **Nuevo**: Recién reportado
- 🔄 **En Progreso**: Se está trabajando en él
- ✅ **Resuelto**: Se ha corregido (pendiente de verificación)
- ✔️ **Cerrado**: Verificado y cerrado
- 🚫 **Rechazado**: No es un defecto o duplicado

#### Panel de Métricas

Verás tarjetas con:

- **Total de Defectos**
- **Defectos Abiertos**
- **Defectos Resueltos**
- **Defectos Cerrados**
- **Por Severidad**: Distribución por severidad
- **Por Tipo**: Distribución por tipo

---

## Configuración Global

**Acceso**: Solo usuarios con rol Admin o Manager.

### Acceder a Configuración

1. Haz clic en **"Configuración"** en la barra de navegación
2. Se abrirá la **página de Configuración Global**

### Secciones de Configuración

La configuración está organizada en pestañas:

#### 1. Roles

Gestiona los roles del sistema:

- Ver roles existentes
- Crear nuevos roles personalizados
- Definir permisos por rol
- Asignar capacidades (crear proyecto, editar artefactos, etc.)

**Crear Rol**:

1. Haz clic en **"+ Nuevo Rol"**
2. Ingresa:
   - Nombre del rol
   - Descripción
   - Permisos (marcar checkboxes)
3. Guarda

#### 2. Fases

Personaliza las fases del proceso:

- Ver fases predefinidas (Inicio, Elaboración, Construcción, Transición)
- Agregar fases personalizadas
- Definir orden de las fases
- Establecer criterios de salida

**Agregar Fase**:

1. Haz clic en **"+ Nueva Fase"**
2. Completa:
   - Nombre de la fase
   - Descripción
   - Orden
   - Requisitos de completitud
3. Guarda

#### 3. Tipos de Artefactos

Define el catálogo de tipos de artefactos:

- Ver tipos existentes por fase
- Crear nuevos tipos de artefactos
- Asociar tipos a fases específicas
- Definir campos personalizados

**Crear Tipo de Artefacto**:

1. Haz clic en **"+ Nuevo Tipo"**
2. Completa:
   - Nombre del tipo
   - Fase asociada
   - Descripción
   - ¿Es obligatorio por defecto?
   - Plantilla (opcional)
3. Guarda

#### 4. Flujos de Trabajo Globales

Define flujos de trabajo reutilizables:

- Crear flujos estándar
- Definir estados y transiciones
- Establecer matriz de permisos

#### 5. Campos Personalizados

Agrega campos adicionales a entidades:

- Proyectos
- Artefactos
- Iteraciones
- Defectos

**Crear Campo Personalizado**:

1. Haz clic en **"+ Nuevo Campo"**
2. Selecciona:
   - Entidad (Proyecto, Artefacto, etc.)
   - Nombre del campo
   - Tipo de dato (Texto, Número, Fecha, Lista, etc.)
   - Obligatorio (Sí/No)
   - Valor por defecto (opcional)
3. Guarda

#### 6. Historial de Cambios

Visualiza todos los cambios en la configuración:

- Fecha y hora del cambio
- Usuario que realizó el cambio
- Tipo de cambio
- Detalles del cambio

### Plantillas OpenUP

Gestiona plantillas de configuración reutilizables:

1. Desde la configuración, haz clic en **"Gestionar Plantillas"**
2. Verás la lista de plantillas disponibles

**Crear Plantilla**:

1. Haz clic en **"+ Nueva Plantilla"**
2. Ingresa:
   - Nombre de la plantilla
   - Descripción
   - Configuración base (selecciona una existente)
3. Personaliza:
   - Fases incluidas
   - Tipos de artefactos
   - Flujos de trabajo
   - Roles
4. Guarda la plantilla

**Aplicar Plantilla a Proyecto**:

1. Al crear un nuevo proyecto
2. Selecciona **"Usar Plantilla"**
3. Elige la plantilla deseada
4. El proyecto se creará con toda la configuración de la plantilla

---

## Notificaciones

El sistema de notificaciones te mantiene informado de eventos importantes.

### Ver Notificaciones

1. Haz clic en el icono de **campana (🔔)** en la barra de navegación
2. Se abrirá un panel desplegable con:
   - Notificaciones no leídas (resaltadas)
   - Notificaciones leídas
   - Contador de notificaciones pendientes

### Tipos de Notificaciones

- 📨 **Invitación a proyecto**: Te han invitado a un proyecto
- ✅ **Cambio de estado**: Un artefacto cambió de estado
- 🔄 **Asignación**: Te asignaron una tarea o defecto
- ⚠️ **Alerta**: Iteración próxima a vencer
- ✔️ **Aprobación**: Un artefacto fue aprobado
- 💬 **Comentario**: Nuevo comentario en un artefacto

### Marcar como Leída

1. Haz clic en una notificación en el panel
2. Se marcará automáticamente como leída
3. O usa **"Marcar todas como leídas"** en la parte inferior

### Configurar Preferencias de Notificaciones

1. Ve a **Perfil > Preferencias**
2. En la sección **"Notificaciones"**:
   - Activa/desactiva tipos de notificaciones
   - Configura notificaciones por email
   - Establece frecuencia de resumen diario

**Opciones disponibles**:

- ✉️ Recibir notificaciones por email
- 📱 Mostrar notificaciones en el navegador
- 📊 Resumen diario de actividad
- 🔕 No molestar (pausar notificaciones)

---

## Invitaciones y Colaboración

### Ver Invitaciones Pendientes

1. Haz clic en **"Invitaciones"** en la barra de navegación
2. Verás una lista de invitaciones pendientes:
   - Proyecto al que te invitan
   - Rol asignado
   - Quien te invitó
   - Fecha de invitación
   - Acciones: Aceptar / Rechazar

### Aceptar una Invitación

1. En la lista de invitaciones, haz clic en **"Aceptar"**
2. Se te agregará al proyecto con el rol especificado
3. El proyecto aparecerá en tu lista de proyectos

### Rechazar una Invitación

1. Haz clic en **"Rechazar"**
2. Confirma la acción
3. La invitación se eliminará

### Invitar Usuarios a un Proyecto

**Nota**: Solo Manager y Admin pueden invitar usuarios.

1. Ve al proyecto
2. Haz clic en **"Miembros del Equipo"**
3. Haz clic en **"+ Invitar Usuario"**
4. Completa:
   - **Email del usuario**: Dirección de correo
   - **Rol**: Selecciona el rol a asignar
   - **Mensaje** (opcional): Mensaje personalizado
5. Haz clic en **"Enviar Invitación"**

El usuario recibirá:

- Una notificación en la aplicación
- Un email con el enlace de invitación

### Gestionar Miembros del Equipo

En la sección **"Miembros del Equipo"**:

**Ver Miembros**:

- Lista de todos los miembros
- Rol de cada miembro
- Estado (Activo, Invitado)
- Acciones disponibles

**Cambiar Rol**:

1. Haz clic en **"Editar"** junto al miembro
2. Selecciona el nuevo rol
3. Guarda los cambios

**Remover Miembro**:

1. Haz clic en **"Remover"** (🗑️)
2. Confirma la acción
3. El usuario perderá acceso al proyecto

---

## Preguntas Frecuentes

### ¿Cómo puedo recuperar mi contraseña?

Contacta al administrador del sistema para que restablezca tu contraseña.

### ¿Puedo participar en varios proyectos a la vez?

Sí, puedes ser miembro de múltiples proyectos con diferentes roles en cada uno.

### ¿Qué formato de archivos puedo subir como artefactos?

Puedes subir documentos (PDF, DOCX, MD), imágenes (PNG, JPG, SVG), archivos comprimidos (ZIP, TAR.GZ) y archivos de código.

### ¿Puedo exportar los datos del proyecto?

Sí, en la configuración del proyecto hay una opción para **"Exportar Proyecto"** que genera un archivo JSON con toda la información.

### ¿Cómo se calculan las métricas de velocidad del equipo?

La velocidad se calcula dividiendo las horas completadas por el número de días de la iteración.

### ¿Puedo personalizar los estados de los artefactos?

Sí, a través de los **Flujos de Trabajo** puedes definir estados personalizados para cada tipo de artefacto.

### ¿Qué sucede si elimino un artefacto?

Los artefactos con versiones aprobadas no pueden eliminarse. Los demás se eliminan permanentemente.

### ¿Puedo restaurar un proyecto archivado?

Sí, en la pestaña **"Proyectos Archivados"**, selecciona el proyecto y haz clic en **"Restaurar"**.

### ¿Cómo se notifica a los miembros del equipo?

El sistema envía notificaciones automáticas dentro de la aplicación y opcionalmente por email si el usuario lo configuró.

### ¿Puedo usar OpenUpTool en dispositivos móviles?

La interfaz es responsive y se adapta a tablets y móviles, aunque la experiencia óptima es en desktop.

### ¿Se puede integrar con sistemas externos?

Sí, la API REST permite integración con sistemas de control de versiones, CI/CD y otras herramientas.

### ¿Hay límite de usuarios o proyectos?

No hay límites técnicos, depende de la configuración del servidor.

### ¿Los datos están respaldados?

Sí, se recomienda que el administrador configure backups periódicos de la base de datos PostgreSQL.

---

## Soporte

Para asistencia adicional:

- 📧 **Email de soporte**: [Contactar al administrador]
- 📚 **Documentación técnica**: Ver ARCHITECTURE.md
- 🐛 **Reportar bugs**: A través del sistema de defectos
- 💡 **Sugerencias**: Contactar al equipo de desarrollo

---

**OpenUpTool v1.0**  
© 2024 - Herramienta de Gestión de Proyectos OpenUP
