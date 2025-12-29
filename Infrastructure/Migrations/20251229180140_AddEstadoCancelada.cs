using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoCancelada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmpleadoId",
                table: "Usuarios");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmpleadoId",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EmpleadoId", "FechaCreacion", "PasswordHash" },
                values: new object[] { null, new DateTime(2025, 12, 29, 11, 13, 9, 458, DateTimeKind.Local).AddTicks(1662), "$2a$11$SIyKkMyZWwKo6YzAZD6I2elyXLg4NB9DJGbFuBxEBAPtJ2tISXaUy" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EmpleadoId", "FechaCreacion", "PasswordHash" },
                values: new object[] { null, new DateTime(2025, 12, 29, 11, 13, 9, 607, DateTimeKind.Local).AddTicks(7328), "$2a$11$4eJWAmjWiyfbcImdowsK7uQVOopuGCEq9bqMPvzzZ5UW7XFAqNUy6" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EmpleadoId", "FechaCreacion", "PasswordHash" },
                values: new object[] { null, new DateTime(2025, 12, 29, 11, 13, 9, 743, DateTimeKind.Local).AddTicks(7107), "$2a$11$oKeA8yPnyfYQAoNMdoICfeOpOltmZV6p9bR6FskfHGTokQ6fSOiWO" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "EmpleadoId", "FechaCreacion", "PasswordHash" },
                values: new object[] { null, new DateTime(2025, 12, 29, 11, 13, 9, 881, DateTimeKind.Local).AddTicks(5621), "$2a$11$/eShXgAC7PLhSEl.NtrNfecMOHUe4z6wmaEObKXLyvplhm23oQfqm" });
        }
    }
}
