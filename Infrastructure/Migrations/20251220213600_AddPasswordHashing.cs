using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHashing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 35, 59, 862, DateTimeKind.Local).AddTicks(7999), "$2a$11$jMcDuFZNXkqpbSinm20vJOU4.oX62Qxy.moe1kVaQh57iyjPO6Ikm" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 35, 59, 989, DateTimeKind.Local).AddTicks(9446), "$2a$11$bXfsMh8lzsiH4.dpJtrsC.Ss0yS45H5nAuvkgDyXbHwmvTlP0o8ce" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 36, 0, 113, DateTimeKind.Local).AddTicks(3312), "$2a$11$AAXlJ6AtmFTRgSXE5uZzjOUyDm8vuG/ckoywoCw/TBiOeP2t.Y2fe" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 12, 54, 222, DateTimeKind.Local).AddTicks(3780), "$2a$11$rJ3Z9YqZX8K8YqZX8K8YqO9YqZX8K8YqZX8K8YqZX8K8YqZX8K8Yq" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 12, 54, 222, DateTimeKind.Local).AddTicks(3784), "$2a$11$rJ3Z9YqZX8K8YqZX8K8YqO9YqZX8K8YqZX8K8YqZX8K8YqZX8K8Yq" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 12, 54, 222, DateTimeKind.Local).AddTicks(3786), "$2a$11$rJ3Z9YqZX8K8YqZX8K8YqO9YqZX8K8YqZX8K8YqZX8K8YqZX8K8Yq" });
        }
    }
}
