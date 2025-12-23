using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_ReadingsRetentionPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "config",
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "ReadingsRetentionPeriod",
                schema: "config",
                table: "Settings");

            migrationBuilder.AddColumn<int>(
                name: "ReadingsRetentionDays",
                schema: "config",
                table: "Settings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadingsRetentionDays",
                schema: "config",
                table: "Settings");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ReadingsRetentionPeriod",
                schema: "config",
                table: "Settings",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.InsertData(
                schema: "config",
                table: "Settings",
                columns: new[] { "Id", "AlertEnabled", "CheckDeviationInterval", "HumidityAlertDeviation", "ReadingsRetentionPeriod", "TempAlertDeviation" },
                values: new object[] { 1, true, new TimeSpan(0, 0, 10, 0, 0), 5.0, new TimeSpan(30, 0, 0, 0, 0), 2.0 });
        }
    }
}
