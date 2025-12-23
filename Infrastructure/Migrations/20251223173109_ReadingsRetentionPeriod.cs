using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReadingsRetentionPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ReadingsRetentionPeriod",
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
                column: "ReadingsRetentionPeriod",
                value: new TimeSpan(30, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadingsRetentionPeriod",
                schema: "config",
                table: "Settings");
        }
    }
}
