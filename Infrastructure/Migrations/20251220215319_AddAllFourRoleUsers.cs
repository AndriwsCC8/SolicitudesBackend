using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllFourRoleUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash" },
                values: new object[] { "superadmin@solicitudes.com", new DateTime(2025, 12, 20, 17, 53, 16, 653, DateTimeKind.Local).AddTicks(8630), "Super Administrador", "superadmin", "$2a$11$xnIGSaPIwwygWUEdnp98zOtVxnkzk18jvLEMpD2XVycVEOzubVUiq" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AreaId", "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { null, "admin@solicitudes.com", new DateTime(2025, 12, 20, 17, 53, 16, 780, DateTimeKind.Local).AddTicks(256), "Administrador", "admin", "$2a$11$6YPWRCXvXhUXl3vod1mEfe4mGphZY/keAAT2Bgbf49Awt2HSR9oC.", 2 });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AreaId", "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { 1, "agente.ti@solicitudes.com", new DateTime(2025, 12, 20, 17, 53, 16, 909, DateTimeKind.Local).AddTicks(9685), "Agente de TI", "agenteti", "$2a$11$jxBapD7Ubv2aTm7tXddhx.6sEqcR4gmC7OMSdgGwb5cHbEYscuyaq", 4 });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "AreaId", "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { 4, true, null, "juan.perez@solicitudes.com", new DateTime(2025, 12, 20, 17, 53, 17, 36, DateTimeKind.Local).AddTicks(6677), "Juan Pérez", "usuario1", "$2a$11$YsxXTwq4AkS4OM3vtPZs5uM5T7qE4IWDPbf7roETIk9az04pHzR1u", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash" },
                values: new object[] { "admin@solicitudes.com", new DateTime(2025, 12, 20, 17, 48, 13, 972, DateTimeKind.Local).AddTicks(3336), "Administrador del Sistema", "admin", "$2a$11$GXczZP2UTldxndx3qfPrdOAgIB/sdtTXqbs0ZPNeccgK2tYUxfGru" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AreaId", "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { 1, "agente.ti@solicitudes.com", new DateTime(2025, 12, 20, 17, 48, 14, 102, DateTimeKind.Local).AddTicks(9887), "Agente de TI", "agenteti", "$2a$11$am1zjzhmdEyd6AMJYntID.G2w6D42NmlagAz/5gSUdjIx50ce/uFy", 4 });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AreaId", "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { null, "juan.perez@solicitudes.com", new DateTime(2025, 12, 20, 17, 48, 14, 237, DateTimeKind.Local).AddTicks(2186), "Juan Pérez", "usuario1", "$2a$11$mzsYM7CzQcL8Z0J7qEatLOFpFTTCMUKI53KlrjGTZfuEms6He18.W", 1 });
        }
    }
}
