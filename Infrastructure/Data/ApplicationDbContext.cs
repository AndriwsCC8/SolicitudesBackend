using Domain.Entities;
using Domain.Enums;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<TipoSolicitud> TiposSolicitud { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<HistorialEstado> HistorialEstados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Rol).IsRequired();

                entity.HasOne(e => e.Area)
                    .WithMany(a => a.Usuarios)
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración Area
            modelBuilder.Entity<Area>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            // Configuración TipoSolicitud
            modelBuilder.Entity<TipoSolicitud>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);

                entity.HasOne(e => e.Area)
                    .WithMany(a => a.TiposSolicitud)
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración Solicitud
            modelBuilder.Entity<Solicitud>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Numero).IsUnique();
                entity.Property(e => e.Asunto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.MotivoRechazo).HasMaxLength(500);

                entity.HasOne(e => e.Area)
                    .WithMany(a => a.Solicitudes)
                    .HasForeignKey(e => e.AreaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TipoSolicitud)
                    .WithMany(t => t.Solicitudes)
                    .HasForeignKey(e => e.TipoSolicitudId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Solicitante)
                    .WithMany(u => u.SolicitudesCreadas)
                    .HasForeignKey(e => e.SolicitanteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.GestorAsignado)
                    .WithMany(u => u.SolicitudesAsignadas)
                    .HasForeignKey(e => e.GestorAsignadoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración Comentario
            modelBuilder.Entity<Comentario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Texto).IsRequired().HasMaxLength(1000);

                entity.HasOne(e => e.Solicitud)
                    .WithMany(s => s.Comentarios)
                    .HasForeignKey(e => e.SolicitudId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Comentarios)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración HistorialEstado
            modelBuilder.Entity<HistorialEstado>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Observacion).HasMaxLength(500);

                entity.HasOne(e => e.Solicitud)
                    .WithMany(s => s.HistorialEstados)
                    .HasForeignKey(e => e.SolicitudId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Datos semilla
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Áreas iniciales
            modelBuilder.Entity<Area>().HasData(
                new Area { Id = 1, Nombre = "TI", Descripcion = "Tecnología de la Información", Activo = true },
                new Area { Id = 2, Nombre = "Mantenimiento", Descripcion = "Mantenimiento de instalaciones", Activo = true },
                new Area { Id = 3, Nombre = "Transporte", Descripcion = "Gestión de transporte", Activo = true },
                new Area { Id = 4, Nombre = "Compras", Descripcion = "Adquisiciones y compras", Activo = true }
            );

            // Tipos de Solicitud iniciales
            modelBuilder.Entity<TipoSolicitud>().HasData(
                new TipoSolicitud { Id = 1, Nombre = "Soporte PC", Descripcion = "Soporte técnico de computadoras", AreaId = 1, Activo = true },
                new TipoSolicitud { Id = 2, Nombre = "Acceso a Sistema", Descripcion = "Solicitud de acceso a sistemas", AreaId = 1, Activo = true },
                new TipoSolicitud { Id = 3, Nombre = "Reparación", Descripcion = "Reparación de instalaciones", AreaId = 2, Activo = true },
                new TipoSolicitud { Id = 4, Nombre = "Asignación de Vehículo", Descripcion = "Solicitud de vehículo", AreaId = 3, Activo = true },
                new TipoSolicitud { Id = 5, Nombre = "Compra de Material", Descripcion = "Solicitud de compra", AreaId = 4, Activo = true }
            );

            // Usuarios iniciales - Uno por cada rol
            // Contraseñas: Super123!, Admin123!, Agente123!, User123!
            // NOTA: Los hashes son fijos para evitar que cambien en cada migración
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NombreUsuario = "superadmin",
                    Nombre = "Super Administrador",
                    Email = "superadmin@solicitudes.com",
                    PasswordHash = "$2a$11$8S/IE0c6cItC8sYEDLFVYeZhPGj5RisDiJx7uubFGaTAqro//9twe", // Super123!
                    Rol = RolEnum.SuperAdministrador,
                    Activo = true,
                    FechaCreacion = new DateTime(2026, 1, 2, 15, 22, 37, 384, DateTimeKind.Local).AddTicks(7335)
                },
                new Usuario
                {
                    Id = 2,
                    NombreUsuario = "admin",
                    Nombre = "Administrador",
                    Email = "admin@solicitudes.com",
                    PasswordHash = "$2a$11$yHk5av1Q9YHOsvYHw/A3W..yr2GQHZH7I4zpiqHPCSdH8yVwNs8Uq", // Admin123!
                    Rol = RolEnum.Administrador,
                    Activo = true,
                    FechaCreacion = new DateTime(2026, 1, 2, 15, 22, 37, 502, DateTimeKind.Local).AddTicks(4322)
                },
                new Usuario
                {
                    Id = 3,
                    NombreUsuario = "agenteti",
                    Nombre = "Agente de TI",
                    Email = "agente.ti@solicitudes.com",
                    PasswordHash = "$2a$11$kPhzQEa1hfUpe7unshNxV./zCMhzst9CggP8cReXDayn/CShyIRyy", // Agente123!
                    Rol = RolEnum.AgenteArea,
                    AreaId = 1,
                    Activo = true,
                    FechaCreacion = new DateTime(2026, 1, 2, 15, 22, 37, 622, DateTimeKind.Local).AddTicks(8622)
                },
                new Usuario
                {
                    Id = 4,
                    NombreUsuario = "usuario1",
                    Nombre = "Juan Pérez",
                    Email = "juan.perez@solicitudes.com",
                    PasswordHash = "$2a$11$NE5Yecuw/F8HYbZF4IlMFu5fCVEGczpVjmAGWRSfIkWagHolkYanm", // User123!
                    Rol = RolEnum.Usuario,
                    Activo = true,
                    FechaCreacion = new DateTime(2026, 1, 2, 15, 22, 37, 740, DateTimeKind.Local).AddTicks(4554)
                }
            );
        }
    }
}