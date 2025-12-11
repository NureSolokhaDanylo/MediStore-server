using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsSolvedAndAddCheckDeviationInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSolved",
                table: "Alerts");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "CheckDeviationInterval",
                schema: "config",
                table: "Settings",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.UpdateData(
                schema: "config",
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CheckDeviationInterval",
                value: new TimeSpan(0, 0, 10, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckDeviationInterval",
                schema: "config",
                table: "Settings");

            migrationBuilder.AddColumn<bool>(
                name: "IsSolved",
                table: "Alerts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
