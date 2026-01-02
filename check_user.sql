-- Verificar estado del usuario 2009
SELECT 
    Id,
    Nombre,
    NombreUsuario,
    Rol,
    CASE Rol
        WHEN 1 THEN 'Usuario'
        WHEN 2 THEN 'Administrador'
        WHEN 3 THEN 'SuperAdministrador'
        WHEN 4 THEN 'AgenteArea'
        ELSE 'Desconocido'
    END AS RolNombre,
    AreaId,
    Activo,
    Email,
    FechaCreacion
FROM Usuarios
WHERE Id = 2009;

-- Ver todos los usuarios con Rol = 4 (AgenteArea)
SELECT 
    Id,
    Nombre,
    NombreUsuario,
    Rol,
    AreaId,
    Activo
FROM Usuarios
WHERE Rol = 4;
