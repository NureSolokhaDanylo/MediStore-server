using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class appsettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TempAlertDeviation = table.Column<double>(type: "float", nullable: false),
                    HumidityAlertDeviation = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.CheckConstraint("CK_AppSettings_Singleton", "[Id] = 1");
                });

            migrationBuilder.InsertData(
                schema: "config",
                table: "Settings",
                columns: new[] { "Id", "AlertEnabled", "HumidityAlertDeviation", "TempAlertDeviation" },
                values: new object[] { 1, true, 5.0, 2.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings",
                schema: "config");
        }
    }
}
