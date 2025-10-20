using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    public partial class AddAccountCompanyRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Thêm cột AccountId vào bảng Company
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Company",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // ✅ Tạo unique index để đảm bảo mỗi company chỉ có 1 account
            migrationBuilder.CreateIndex(
                name: "IX_Company_AccountId",
                table: "Company",
                column: "AccountId",
                unique: true);

            // ✅ Tạo foreign key liên kết Company -> Account
            migrationBuilder.AddForeignKey(
                name: "FK_Company_Account_AccountId",
                table: "Company",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_Account_AccountId",
                table: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Company_AccountId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Company");
        }
    }
}
