using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReadingsZoneRel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Sensors_SensorId",
                table: "Readings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppSettings_Singleton",
                schema: "config",
                table: "Settings");

            migrationBuilder.AlterColumn<int>(
                name: "SensorId",
                table: "Readings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ZoneId",
                table: "Readings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readings_ZoneId",
                table: "Readings",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Readings_Sensors_SensorId",
                table: "Readings",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Readings_Zones_ZoneId",
                table: "Readings",
                column: "ZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Sensors_SensorId",
                table: "Readings");

            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Zones_ZoneId",
                table: "Readings");

            migrationBuilder.DropIndex(
                name: "IX_Readings_ZoneId",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "ZoneId",
                table: "Readings");

            migrationBuilder.AlterColumn<int>(
                name: "SensorId",
                table: "Readings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppSettings_Singleton",
                schema: "config",
                table: "Settings",
                sql: "[Id] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Readings_Sensors_SensorId",
                table: "Readings",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
