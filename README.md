SolicitudesBackend

Backend del Sistema de Solicitudes Internas, desarrollado en ASP.NET Core 8 siguiendo Clean Architecture y buenas prácticas profesionales (JWT, roles, GitFlow, validaciones de negocio).

📑 PARTE I – README (PARA EL REPOSITORIO)
🚀 Descripción

Sistema backend para la gestión de solicitudes internas de una organización. Permite a los usuarios crear solicitudes, a los agentes de área gestionarlas, a los administradores supervisarlas y al SuperAdministrador controlar completamente los usuarios del sistema.

🏗️ Arquitectura

Clean Architecture (4 capas):

Api: Controllers, Middlewares, configuración

Application: DTOs, Interfaces

Domain: Entidades, Enums, Excepciones de dominio

Infrastructure: EF Core, DbContext, Servicios

🔐 Seguridad

Autenticación JWT Bearer

Claims personalizados (UserId, Role, AreaId)

Hash de contraseñas con BCrypt

Middleware global de excepciones

Autorización por roles

Roles del sistema:

Usuario

AgenteArea

Administrador

SuperAdministrador (gestión exclusiva de usuarios)

🧠 Funcionalidades Principales
📄 Solicitudes

Crear solicitud

Asignar agente

Cambiar estado (workflow controlado)

Rechazar con motivo

Cerrar solicitud

🔄 Workflow de Estados

Nueva → EnProceso → Resuelta → Cerrada

Cada cambio queda registrado en el historial.

💬 Comentarios

Agregar comentarios a solicitudes

Consultar comentarios por solicitud

Validación por rol y área

🕓 Historial

Registro automático de cambios de estado

Consulta de historial por solicitud

� Sistema de Comentarios

Agregar comentarios a solicitudes

Visibilidad según rol (solicitante, gestor, admin)

Consultar historial de comentarios

📊 Módulo de Administración (Administrador y SuperAdministrador)

👥 Gestión de Usuarios (Solo SuperAdministrador)
- CRUD completo de usuarios
- Asignación de roles y áreas
- Activación/desactivación
- Validaciones de negocio

🏢 Gestión de Áreas (Admin y SuperAdmin)
- CRUD completo de áreas
- Contadores de agentes y solicitudes
- Protección contra eliminación con dependencias

📁 Gestión de Categorías (Admin y SuperAdmin)
- CRUD completo de tipos de solicitud
- Vinculación con áreas
- Seguimiento de uso

📈 Reportes en Tiempo Real (Admin y SuperAdmin)
- Reporte resumen general
- Métricas por área
- Desempeño de agentes
- Análisis de tiempos de respuesta y SLA

🗄️ Base de Datos

Entity Framework Core 8

SQL Server

Migraciones aplicadas



Para ejecutar el backend: entrar en la carpeta del proyecto "SolicitudesBackend"

luego entrar en la api "cd api"

luego ejecutar "dotnet run"

entrar en la url que esta en la terminal para probar el swagger.


