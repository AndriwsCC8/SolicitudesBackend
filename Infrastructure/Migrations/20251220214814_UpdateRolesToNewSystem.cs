using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRolesToNewSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "Nombre", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 48, 13, 972, DateTimeKind.Local).AddTicks(3336), "Administrador del Sistema", "$2a$11$GXczZP2UTldxndx3qfPrdOAgIB/sdtTXqbs0ZPNeccgK2tYUxfGru" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { "agente.ti@solicitudes.com", new DateTime(2025, 12, 20, 17, 48, 14, 102, DateTimeKind.Local).AddTicks(9887), "Agente de TI", "agenteti", "$2a$11$am1zjzhmdEyd6AMJYntID.G2w6D42NmlagAz/5gSUdjIx50ce/uFy", 4 });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "NombreUsuario", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 48, 14, 237, DateTimeKind.Local).AddTicks(2186), "usuario1", "$2a$11$mzsYM7CzQcL8Z0J7qEatLOFpFTTCMUKI53KlrjGTZfuEms6He18.W" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "Nombre", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 35, 59, 862, DateTimeKind.Local).AddTicks(7999), "Administrador", "$2a$11$jMcDuFZNXkqpbSinm20vJOU4.oX62Qxy.moe1kVaQh57iyjPO6Ikm" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Email", "FechaCreacion", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { "gestor.ti@solicitudes.com", new DateTime(2025, 12, 20, 17, 35, 59, 989, DateTimeKind.Local).AddTicks(9446), "Gestor TI", "gestorti", "$2a$11$bXfsMh8lzsiH4.dpJtrsC.Ss0yS45H5nAuvkgDyXbHwmvTlP0o8ce", 2 });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "NombreUsuario", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 36, 0, 113, DateTimeKind.Local).AddTicks(3312), "solicitante1", "$2a$11$AAXlJ6AtmFTRgSXE5uZzjOUyDm8vuG/ckoywoCw/TBiOeP2t.Y2fe" });
        }
    }
}
