using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStationFromPricingRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⚙️ Xóa index (nếu có)
            migrationBuilder.DropIndex(
                name: "IX_PricingRules_StationId",
                table: "PricingRules");

            // ⚙️ Xóa cột StationId khỏi bảng PricingRules
            migrationBuilder.DropColumn(
                name: "StationId",
                table: "PricingRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ⚙️ Thêm lại cột khi rollback migration
            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "PricingRules",
                type: "int",
                nullable: true);

            // ⚙️ Tạo lại index
            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_StationId",
                table: "PricingRules",
                column: "StationId");

            // ⚙️ (Không thêm lại ForeignKey vì không còn cần thiết)
        }
    }
}
