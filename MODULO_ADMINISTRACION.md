# Módulo de Administración - Sistema de Solicitudes

## Descripción General

El módulo de administración proporciona funcionalidades completas para la gestión del sistema, incluyendo:
- Gestión de usuarios
- Gestión de áreas
- Gestión de categorías/tipos de solicitud
- Generación de reportes y análisis

## Control de Acceso

### Roles y Permisos

| Funcionalidad | SuperAdministrador | Administrador | Usuario/Agente |
|--------------|-------------------|---------------|----------------|
| Gestión de Usuarios | ✅ | ❌ | ❌ |
| Gestión de Áreas | ✅ | ✅ | ❌ |
| Gestión de Categorías | ✅ | ✅ | ❌ |
| Reportes | ✅ | ✅ | ❌ |

## Endpoints

### Base URL: `http://localhost:5024/api/admin`

---

## 1. Gestión de Usuarios

### `GET /usuarios`
Obtiene lista de todos los usuarios del sistema.

**Autorización:** `SuperAdministrador`

**Respuesta:**
```json
[
  {
    "id": 1,
    "nombreUsuario": "superadmin",
    "nombre": "Super Administrador",
    "email": "superadmin@example.com",
    "rol": 3,
    "rolNombre": "Super Administrador",
    "areaId": null,
    "areaNombre": null,
    "activo": true,
    "fechaCreacion": "2024-12-20T21:53:19.000Z"
  }
]
```

---

### `GET /usuarios/{id}`
Obtiene un usuario específico por ID.

**Autorización:** `SuperAdministrador`

**Parámetros:**
- `id` (path): ID del usuario

**Respuesta:**
```json
{
  "id": 1,
  "nombreUsuario": "superadmin",
  "nombre": "Super Administrador",
  "email": "superadmin@example.com",
  "rol": 3,
  "rolNombre": "Super Administrador",
  "areaId": null,
  "areaNombre": null,
  "activo": true,
  "fechaCreacion": "2024-12-20T21:53:19.000Z"
}
```

---

### `POST /usuarios`
Crea un nuevo usuario.

**Autorización:** `SuperAdministrador`

**Body:**
```json
{
  "nombreUsuario": "nuevo.usuario",
  "nombre": "Usuario Nuevo",
  "email": "usuario.nuevo@example.com",
  "password": "Password123!",
  "rol": 1,
  "areaId": null
}
```

**Campos:**
- `nombreUsuario` (string, requerido): Nombre de usuario único
- `nombre` (string, requerido): Nombre completo
- `email` (string, requerido): Email único
- `password` (string, requerido): Contraseña
- `rol` (int, requerido): 1=Usuario, 2=Administrador, 3=SuperAdministrador, 4=AgenteArea
- `areaId` (int, opcional): ID del área (requerido si rol=4)

**Validaciones:**
- Email único en el sistema
- Nombre de usuario único
- AgenteArea (rol=4) debe tener área asignada
- El área debe existir

**Respuesta:** `201 Created`
```json
{
  "id": 5,
  "nombreUsuario": "nuevo.usuario",
  "nombre": "Usuario Nuevo",
  "email": "usuario.nuevo@example.com",
  "rol": 1,
  "rolNombre": "Usuario",
  "areaId": null,
  "areaNombre": null,
  "activo": true,
  "fechaCreacion": "2024-12-20T22:00:00.000Z"
}
```

---

### `PUT /usuarios/{id}`
Actualiza un usuario existente.

**Autorización:** `SuperAdministrador`

**Parámetros:**
- `id` (path): ID del usuario

**Body (todos los campos opcionales):**
```json
{
  "nombre": "Nombre Actualizado",
  "email": "nuevo.email@example.com",
  "password": "NuevaPassword123!",
  "rol": 2,
  "areaId": 1,
  "activo": true
}
```

**Validaciones:**
- Email único (si se cambia)
- Si cambia a AgenteArea debe tener área

**Respuesta:**
```json
{
  "id": 5,
  "nombreUsuario": "nuevo.usuario",
  "nombre": "Nombre Actualizado",
  "email": "nuevo.email@example.com",
  "rol": 2,
  "rolNombre": "Administrador",
  "areaId": 1,
  "areaNombre": "Tecnología",
  "activo": true,
  "fechaCreacion": "2024-12-20T22:00:00.000Z"
}
```

---

### `DELETE /usuarios/{id}`
Elimina un usuario.

**Autorización:** `SuperAdministrador`

**Parámetros:**
- `id` (path): ID del usuario

**Restricciones:**
- No se puede eliminar el SuperAdministrador principal (ID=1)
- No se puede eliminar usuarios con solicitudes asignadas (desactivar en su lugar)

**Respuesta:** `200 OK`
```json
{
  "mensaje": "Usuario eliminado correctamente"
}
```

---

## 2. Gestión de Áreas

### `GET /areas`
Obtiene lista de todas las áreas.

**Autorización:** `Administrador, SuperAdministrador`

**Respuesta:**
```json
[
  {
    "id": 1,
    "nombre": "Tecnología",
    "descripcion": "Área de tecnología y sistemas",
    "activo": true,
    "cantidadAgentes": 3,
    "cantidadSolicitudes": 25
  }
]
```

---

### `GET /areas/{id}`
Obtiene un área específica.

**Autorización:** `Administrador, SuperAdministrador`

**Respuesta:** Ver formato arriba

---

### `POST /areas`
Crea una nueva área.

**Autorización:** `Administrador, SuperAdministrador`

**Body:**
```json
{
  "nombre": "Recursos Humanos",
  "descripcion": "Gestión de recursos humanos"
}
```

**Validaciones:**
- Nombre único

**Respuesta:** `201 Created`

---

### `PUT /areas/{id}`
Actualiza un área.

**Autorización:** `Administrador, SuperAdministrador`

**Body (campos opcionales):**
```json
{
  "nombre": "Tecnología de la Información",
  "descripcion": "Soporte técnico y desarrollo",
  "activo": true
}
```

---

### `DELETE /areas/{id}`
Elimina un área.

**Autorización:** `Administrador, SuperAdministrador`

**Restricciones:**
- No se puede eliminar si tiene solicitudes asociadas
- No se puede eliminar si tiene usuarios asignados

**Respuesta:** `200 OK`

---

## 3. Gestión de Categorías

### `GET /categorias`
Obtiene todas las categorías/tipos de solicitud.

**Autorización:** `Administrador, SuperAdministrador`

**Respuesta:**
```json
[
  {
    "id": 1,
    "nombre": "Soporte Técnico",
    "descripcion": "Soporte técnico general",
    "areaId": 1,
    "areaNombre": "Tecnología",
    "activo": true,
    "cantidadSolicitudes": 15
  }
]
```

---

### `GET /categorias/{id}`
Obtiene una categoría específica.

**Autorización:** `Administrador, SuperAdministrador`

---

### `POST /categorias`
Crea una nueva categoría.

**Autorización:** `Administrador, SuperAdministrador`

**Body:**
```json
{
  "nombre": "Soporte de Aplicaciones",
  "descripcion": "Problemas con aplicaciones",
  "areaId": 1
}
```

**Validaciones:**
- Nombre único por área
- El área debe existir

---

### `PUT /categorias/{id}`
Actualiza una categoría.

**Autorización:** `Administrador, SuperAdministrador`

**Body (campos opcionales):**
```json
{
  "nombre": "Soporte Técnico General",
  "descripcion": "Actualización de descripción",
  "areaId": 1,
  "activo": true
}
```

---

### `DELETE /categorias/{id}`
Elimina una categoría.

**Autorización:** `Administrador, SuperAdministrador`

**Restricciones:**
- No se puede eliminar si tiene solicitudes asociadas (desactivar en su lugar)

---

## 4. Reportes

### `GET /reportes/resumen`
Obtiene reporte resumen general del sistema.

**Autorización:** `Administrador, SuperAdministrador`

**Respuesta:**
```json
{
  "totalSolicitudes": 100,
  "solicitudesNuevas": 15,
  "solicitudesEnProceso": 30,
  "solicitudesResueltas": 40,
  "solicitudesCerradas": 10,
  "solicitudesRechazadas": 3,
  "solicitudesCanceladas": 2,
  "tiempoPromedioResolucion": 24.5,
  "usuariosActivos": 15,
  "totalAreas": 4
}
```

**Campos:**
- `tiempoPromedioResolucion`: Tiempo en horas

---

### `GET /reportes/por-area`
Obtiene métricas por área.

**Autorización:** `Administrador, SuperAdministrador`

**Respuesta:**
```json
[
  {
    "areaId": 1,
    "areaNombre": "Tecnología",
    "totalSolicitudes": 50,
    "solicitudesAbiertas": 20,
    "solicitudesResueltas": 30,
    "cantidadAgentes": 5,
    "tiempoPromedioResolucion": 18.5
  }
]
```

---

### `GET /reportes/desempeno-agentes`
Obtiene métricas de desempeño por agente.

**Autorización:** `Administrador, SuperAdministrador`

**Respuesta:**
```json
[
  {
    "agenteId": 4,
    "agenteNombre": "Juan Pérez",
    "areaNombre": "Tecnología",
    "solicitudesAsignadas": 25,
    "solicitudesResueltas": 20,
    "solicitudesEnProceso": 5,
    "tasaResolucion": 80.0,
    "tiempoPromedioResolucion": 16.5
  }
]
```

**Campos:**
- `tasaResolucion`: Porcentaje de resolución
- `tiempoPromedioResolucion`: Tiempo en horas

---

### `GET /reportes/tiempos-respuesta`
Obtiene análisis de tiempos de respuesta.

**Autorización:** `Administrador, SuperAdministrador`

**Respuesta:**
```json
{
  "tiempoPromedioTotal": 24.5,
  "tiempoPromedioNuevaAEnProceso": 2.5,
  "tiempoPromedioEnProcesoAResuelta": 22.0,
  "tiempoMinimoResolucion": 1.5,
  "tiempoMaximoResolucion": 120.0,
  "solicitudesFueraDeSLA": 5
}
```

**Campos:**
- Todos los tiempos en horas
- `solicitudesFueraDeSLA`: Solicitudes con más de 72 horas sin cerrar

---

## Códigos de Estado

| Código | Descripción |
|--------|-------------|
| 200 | OK - Operación exitosa |
| 201 | Created - Recurso creado |
| 400 | Bad Request - Error de validación |
| 401 | Unauthorized - No autenticado |
| 403 | Forbidden - Sin permisos |
| 404 | Not Found - Recurso no encontrado |
| 500 | Internal Server Error |

## Errores Comunes

### 400 Bad Request
```json
{
  "mensaje": "El email ya está registrado"
}
```

### 403 Forbidden
```json
{
  "mensaje": "No tiene permisos para acceder a este recurso"
}
```

### 404 Not Found
```json
{
  "mensaje": "Usuario no encontrado"
}
```

## Notas Importantes

1. **Seguridad:**
   - Todas las contraseñas se hashean con BCrypt
   - Los tokens JWT deben incluirse en el header `Authorization: Bearer {token}`
   - Las contraseñas nunca se exponen en las respuestas

2. **Validaciones:**
   - Emails y usernames deben ser únicos
   - No se permite eliminar recursos con dependencias
   - El SuperAdministrador principal no puede ser eliminado

3. **Reportes:**
   - Los tiempos se calculan en horas
   - Solo se consideran solicitudes con fecha de cierre para promedios
   - Las tasas se expresan en porcentajes

4. **Áreas y Categorías:**
   - Si no se pueden eliminar por dependencias, se recomienda desactivar
   - Las categorías son específicas por área
