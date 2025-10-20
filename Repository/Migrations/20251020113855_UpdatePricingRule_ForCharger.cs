using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    public partial class UpdatePricingRule_ForCharger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔹 Đổi tên cột VehicleType → ChargerType (bảng PricingRules)
            migrationBuilder.RenameColumn(
                name: "VehicleType",
                table: "PricingRules", // 👈 chỉnh lại đúng tên bảng thật
                newName: "ChargerType");

            // 🔹 Thêm cột PowerKw
            migrationBuilder.AddColumn<decimal>(
                name: "PowerKw",
                table: "PricingRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PowerKw",
                table: "PricingRules");

            migrationBuilder.RenameColumn(
                name: "ChargerType",
                table: "PricingRules",
                newName: "VehicleType");
        }
    }
}
