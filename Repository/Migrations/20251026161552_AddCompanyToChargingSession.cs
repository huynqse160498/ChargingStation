using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyToChargingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "ChargingSession",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSession_CompanyId",
                table: "ChargingSession",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingSession_Company_CompanyId",
                table: "ChargingSession",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_Company_CompanyId",
                table: "ChargingSession");

            migrationBuilder.DropIndex(
                name: "IX_ChargingSession_CompanyId",
                table: "ChargingSession");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ChargingSession");
        }
    }
}
