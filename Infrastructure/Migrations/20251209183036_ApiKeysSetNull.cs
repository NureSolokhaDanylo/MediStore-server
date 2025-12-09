using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApiKeysSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorApiKeys_Sensors_SensorId",
                table: "SensorApiKeys");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorApiKeys_Sensors_SensorId",
                table: "SensorApiKeys",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorApiKeys_Sensors_SensorId",
                table: "SensorApiKeys");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorApiKeys_Sensors_SensorId",
                table: "SensorApiKeys",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id");
        }
    }
}
