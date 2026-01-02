using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeAreaIdNullableInSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Solicitudes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 15, 22, 37, 384, DateTimeKind.Local).AddTicks(7335), "$2a$11$8S/IE0c6cItC8sYEDLFVYeZhPGj5RisDiJx7uubFGaTAqro//9twe" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 15, 22, 37, 502, DateTimeKind.Local).AddTicks(4322), "$2a$11$yHk5av1Q9YHOsvYHw/A3W..yr2GQHZH7I4zpiqHPCSdH8yVwNs8Uq" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 15, 22, 37, 622, DateTimeKind.Local).AddTicks(8622), "$2a$11$kPhzQEa1hfUpe7unshNxV./zCMhzst9CggP8cReXDayn/CShyIRyy" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2026, 1, 2, 15, 22, 37, 740, DateTimeKind.Local).AddTicks(4554), "$2a$11$NE5Yecuw/F8HYbZF4IlMFu5fCVEGczpVjmAGWRSfIkWagHolkYanm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AreaId",
                table: "Solicitudes",
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
        }
    }
}
