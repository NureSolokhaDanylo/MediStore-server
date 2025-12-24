using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlertUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Sensors_SensorId",
                table: "Alerts");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Sensors_SensorId",
                table: "Alerts",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Sensors_SensorId",
                table: "Alerts");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Sensors_SensorId",
                table: "Alerts",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
