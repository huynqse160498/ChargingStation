using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentRelationToChargingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargingSessionId",
                table: "Payment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ChargingSessionId",
                table: "Payment",
                column: "ChargingSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_ChargingSession_ChargingSessionId",
                table: "Payment",
                column: "ChargingSessionId",
                principalTable: "ChargingSession",
                principalColumn: "ChargingSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_ChargingSession_ChargingSessionId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_ChargingSessionId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ChargingSessionId",
                table: "Payment");
        }
    }
}
