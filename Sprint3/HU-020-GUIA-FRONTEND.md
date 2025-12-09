# HU-020: Guía de Uso desde el Frontend

## 📋 Funcionalidad: Reasignar Entregables entre Fases o Flujos

Esta historia de usuario permite a los usuarios con permisos (Admin, Manager, Developer) reasignar artefactos entre diferentes fases del proyecto, con validaciones automáticas y registro de historial.

---

## 🚀 Cómo Acceder a la Funcionalidad

### Paso 1: Navegar a un Proyecto

1. Inicia sesión en OpenUpTool
2. Ve a la página de **Proyectos** (`/projects`)
3. Selecciona un proyecto existente

### Paso 2: Acceder a la Vista de Artefactos

Desde la página del proyecto, tienes varias opciones:

#### Opción A: Vista de Fase Específica

```
/projects/{projectId}/inception
/projects/{projectId}/elaboration
/projects/{projectId}/construction
/projects/{projectId}/testing
```

#### Opción B: Vista de Iteración

```
/projects/{projectId}/iterations/{iterationId}
```

### Paso 3: Localizar el Botón de Reasignación

1. En la tabla de artefactos, cada fila tiene una columna de **Acciones**
2. Busca el botón **"Reasignar"** con el ícono de intercambio (⇄)
3. El botón solo aparece si tienes permisos de Admin, Manager o Developer

---

## 🎯 Usando el Diálogo de Reasignación

### Campos del Formulario

#### 1. **Información del Artefacto** (Solo lectura)

-   Muestra el título del artefacto
-   Indica la fase actual
-   Muestra el tipo de artefacto

#### 2. **Fase Destino** (Requerido)

-   Selector desplegable con las fases disponibles
-   Excluye automáticamente la fase actual
-   Opciones:
    -   Inception
    -   Elaboration
    -   Construction
    -   Transition

#### 3. **Validación Automática**

Al seleccionar una fase destino, el sistema automáticamente:

-   ✅ Valida el movimiento
-   ⚠️ Muestra advertencias (warnings) si aplica
-   ❌ Muestra errores si el movimiento no es permitido

**Tipos de Validaciones:**

-   **BACKWARD_PHASE_MOVE** (Warning): Retroceso en el ciclo de vida
-   **SKIP_PHASE** (Info): Se saltan fases intermedias
-   **INVALID_PHASE** (Error): Fase no válida
-   Otras validaciones según reglas de negocio

#### 4. **Razón de Reasignación** (Requerido)

-   Campo de texto multilinea
-   Mínimo 10 caracteres
-   Debe explicar brevemente el motivo del cambio

#### 5. **Forzar Reasignación** (Opcional)

-   Checkbox que aparece solo si hay violaciones
-   Permite proceder a pesar de warnings
-   Los errores críticos no pueden ser forzados

---

## 📊 Ejemplos de Uso

### Caso 1: Movimiento Normal (Sin Violaciones)

```
Artefacto: "Vision Document v1.0"
Fase Actual: INCEPTION
Fase Destino: ELABORATION
Razón: "El documento de visión ha sido aprobado y estamos avanzando a elaboración"

Resultado: ✅ Movimiento exitoso sin advertencias
```

### Caso 2: Retroceso con Confirmación

```
Artefacto: "Architecture Document v2.0"
Fase Actual: CONSTRUCTION
Fase Destino: ELABORATION
Razón: "Se requieren cambios significativos en la arquitectura"

Validación: ⚠️ BACKWARD_PHASE_MOVE
Acción: Marcar checkbox "Forzar reasignación"
Resultado: ✅ Movimiento exitoso con advertencia registrada
```

### Caso 3: Salto de Fases

```
Artefacto: "Requirements Document"
Fase Actual: INCEPTION
Fase Destino: CONSTRUCTION
Razón: "Proyecto urgente, se aprobó omitir elaboración"

Validación: ⚠️ SKIP_PHASE
Acción: Marcar checkbox "Forzar reasignación"
Resultado: ✅ Movimiento exitoso con advertencia registrada
```

---

## 🔍 Visualizando el Historial de Movimientos

### Desde la Vista de Artefactos

1. Haz clic en el botón de expansión (▶) en la fila del artefacto
2. Selecciona la pestaña **"Versiones"** o una futura pestaña de **"Historial"**
3. Verás todos los movimientos realizados:
    - Fecha y hora del movimiento
    - Usuario que realizó el cambio
    - Fase origen → Fase destino
    - Razón del movimiento
    - Si hubo violaciones confirmadas

### Información del Historial

Cada entrada muestra:

```json
{
    "id": "uuid",
    "artifactId": "uuid",
    "fromPhaseId": "INCEPTION",
    "toPhaseId": "ELABORATION",
    "reason": "Documento aprobado",
    "movedBy": "admin@openuptool.com",
    "movedAt": "2025-12-09T06:28:08Z",
    "hadViolations": false
}
```

---

## 🛠️ Flujo Completo de Trabajo

### Flujo Estándar

```
1. Usuario navega a la página de proyecto
   ↓
2. Selecciona una fase (ej: /projects/{id}/inception)
   ↓
3. Identifica el artefacto a reasignar
   ↓
4. Hace clic en el botón "Reasignar"
   ↓
5. Se abre el diálogo de reasignación
   ↓
6. Selecciona la fase destino
   ↓
7. El sistema valida automáticamente
   ↓
8. Si hay warnings: decide si forzar o cancelar
   ↓
9. Ingresa la razón (min. 10 caracteres)
   ↓
10. Hace clic en "Reasignar"
   ↓
11. El sistema confirma el éxito
   ↓
12. La tabla se actualiza automáticamente
```

---

## 🔐 Permisos Requeridos

### Roles que Pueden Reasignar

-   ✅ **Admin**: Acceso completo
-   ✅ **Manager**: Puede reasignar artefactos en sus proyectos
-   ✅ **Developer**: Puede reasignar artefactos
-   ❌ **Viewer**: Solo lectura, no puede reasignar

---

## 📝 Reglas de Negocio

### Validaciones Automáticas

1. **No se puede reasignar a la misma fase**

    - El selector excluye la fase actual

2. **Retrocesos generan advertencias**

    - Mover de Construction → Elaboration
    - Mover de Transition → Construction
    - Requiere confirmación explícita

3. **Saltos de fase generan advertencias**

    - Mover de Inception → Construction
    - Mover de Inception → Transition
    - Requiere confirmación explícita

4. **La razón es obligatoria**

    - Mínimo 10 caracteres
    - Debe ser descriptiva

5. **Todo queda registrado**
    - Usuario que realizó el cambio
    - Fecha y hora exacta
    - Razón del movimiento
    - Si hubo violaciones confirmadas

---

## 🎨 Componentes del Frontend

### Archivos Involucrados

1. **Tipos TypeScript**

    ```typescript
    // src/types/artifact.ts
    -ValidateReassignmentInput -
        ReassignmentValidationResult -
        ReassignArtifactInput -
        ReassignmentResult -
        MovementHistory;
    ```

2. **Servicio de API**

    ```typescript
    // src/services/artifactService.ts
    -validateReassignment() -
        reassignArtifact() -
        reassignWorkflow() -
        getMovementHistory();
    ```

3. **Componentes React**

    ```typescript
    // src/components/artifacts/ReassignArtifactDialog.tsx
    - Diálogo principal de reasignación

    // src/components/artifacts/PhaseArtifactsView.tsx
    - Vista de artefactos con botón de reasignar
    ```

---

## 🧪 Probando la Funcionalidad

### Prueba Rápida

1. Inicia sesión como admin@openuptool.com
2. Ve a `/projects` y selecciona un proyecto
3. Navega a `/projects/{id}/inception`
4. Crea un artefacto de prueba
5. Haz clic en "Reasignar"
6. Selecciona "Elaboration" como destino
7. Ingresa razón: "Prueba de reasignación"
8. Haz clic en "Reasignar"
9. Verifica que el artefacto cambió de fase

### Script de Pruebas Backend

```powershell
# Desde PowerShell
cd E:\Escritorio\openuptool-api\Sprint3
.\HU-020-Reasignar-Entregables.ps1
```

---

## 🐛 Solución de Problemas

### Problema: No veo el botón "Reasignar"

**Solución**: Verifica que tengas permisos de Admin, Manager o Developer

### Problema: El diálogo no valida automáticamente

**Solución**: Asegúrate de haber seleccionado una fase destino diferente a la actual

### Problema: No puedo forzar la reasignación

**Solución**: Solo los warnings pueden ser forzados. Los errores críticos bloquean el movimiento.

### Problema: El historial no se muestra

**Solución**: Verifica que el backend esté corriendo y que tengas conexión a la API

---

## 📞 Contacto y Soporte

Para más información o reportar problemas:

-   Documentación Backend: `ARCHITECTURE.md`
-   Documentación de Tests: `PHASE_VALIDATION_TESTS.md`
-   Script de Pruebas: `HU-020-Reasignar-Entregables.ps1`

---

## ✅ Checklist de Funcionalidad

-   [x] Validación antes de reasignar
-   [x] Reasignación entre fases
-   [x] Detección de retrocesos
-   [x] Detección de saltos de fase
-   [x] Confirmación de violaciones
-   [x] Razón obligatoria
-   [x] Historial de movimientos
-   [x] Registro de usuario y fecha
-   [x] Permisos por rol
-   [x] UI intuitiva con Material-UI
