using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpleadoIdToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmpleadoId",
                table: "Usuarios");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 53, 16, 653, DateTimeKind.Local).AddTicks(8630), "$2a$11$xnIGSaPIwwygWUEdnp98zOtVxnkzk18jvLEMpD2XVycVEOzubVUiq" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 53, 16, 780, DateTimeKind.Local).AddTicks(256), "$2a$11$6YPWRCXvXhUXl3vod1mEfe4mGphZY/keAAT2Bgbf49Awt2HSR9oC." });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 53, 16, 909, DateTimeKind.Local).AddTicks(9685), "$2a$11$jxBapD7Ubv2aTm7tXddhx.6sEqcR4gmC7OMSdgGwb5cHbEYscuyaq" });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "FechaCreacion", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 17, 53, 17, 36, DateTimeKind.Local).AddTicks(6677), "$2a$11$YsxXTwq4AkS4OM3vtPZs5uM5T7qE4IWDPbf7roETIk9az04pHzR1u" });
        }
    }
}
