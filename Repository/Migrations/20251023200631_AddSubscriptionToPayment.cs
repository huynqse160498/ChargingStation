using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceYearly",
                table: "SubscriptionPlan");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionId",
                table: "Payment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_SubscriptionId",
                table: "Payment",
                column: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Subscription_SubscriptionId",
                table: "Payment",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Subscription_SubscriptionId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_SubscriptionId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Payment");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceYearly",
                table: "SubscriptionPlan",
                type: "decimal(12,2)",
                nullable: true);
        }
    }
}
