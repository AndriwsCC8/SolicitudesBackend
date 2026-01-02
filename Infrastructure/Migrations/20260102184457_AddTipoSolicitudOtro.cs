using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoSolicitudOtro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "TiposSolicitud",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 14, 44, 56, 349, DateTimeKind.Local).AddTicks(5440), "$2a$11$i7HDm/vaM2rkgEZDOYtsjuLUMJN3U/ItZb4zYBJLyFcxTFry0iVsS" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 14, 44, 56, 470, DateTimeKind.Local).AddTicks(2583), "$2a$11$1ZeEH4vBpRLq2xTed/C/8.9Aq9C7CffqZn2WDSL5bvbfJJhu81.l." });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 14, 44, 56, 592, DateTimeKind.Local).AddTicks(6019), "$2a$11$.L5dxlS8.SFI3cSMZFhuAeUI2KWOe2c0P2TNPxf1YpedS0RBWefBe" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 14, 44, 56, 720, DateTimeKind.Local).AddTicks(6840), "$2a$11$fFqW93HZmNtL8HxM6MkwsuRBFRgEA3c.QLp0B671vp.iP8Y6tt7k." });

            // Agregar tipo de solicitud "Otro" sin área específica
            migrationBuilder.InsertData(
                table: "TiposSolicitud",
                columns: new[] { "Nombre", "Descripcion", "AreaId", "Activo" },
                values: new object[] { "Otro", "Solicitud general que requiere clasificación administrativa", null, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "TiposSolicitud",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 29, 14, 1, 37, 628, DateTimeKind.Local).AddTicks(7059), "$2a$11$LcsnDSQyOhIPkwnZQuX1VOIb57z9UsoYUKH7sWyczAqrnTOIChl9G" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 29, 14, 1, 37, 757, DateTimeKind.Local).AddTicks(6337), "$2a$11$zrwo1c/5mfvs6V8DojymYODH7tTFUn4MZh.fE9ylgxhcwKt9L9DuW" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 29, 14, 1, 37, 893, DateTimeKind.Local).AddTicks(9117), "$2a$11$.jjczi9.2ap/YWa9eHqexOYNfM0WJvyq/6MZPnTrqcbHjek0ge7wW" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 29, 14, 1, 38, 32, DateTimeKind.Local).AddTicks(2523), "$2a$11$h2Meyg30TVChcUpjHNds4uHGMTNRL9Bd6TL2zJAaYnZDeRFKJzdFK" });
        }
    }
}
