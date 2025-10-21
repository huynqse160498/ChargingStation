using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyBookingRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🧱 Thêm cột CompanyId vào bảng Booking
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Booking",
                type: "int",
                nullable: true);

            // 🔗 Tạo index cho CompanyId
            migrationBuilder.CreateIndex(
                name: "IX_Booking_CompanyId",
                table: "Booking",
                column: "CompanyId");

            // 🔗 Tạo foreign key từ Booking → Company
            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Company_CompanyId",
                table: "Booking",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ❌ Xóa FK, index, và cột khi rollback migration
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Company_CompanyId",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_CompanyId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Booking");
        }
    }
}
