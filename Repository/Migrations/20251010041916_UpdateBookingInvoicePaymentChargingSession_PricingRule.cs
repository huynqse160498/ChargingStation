using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    public partial class UpdateBookingInvoicePaymentChargingSession_PricingRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo bảng PricingRule mới
            migrationBuilder.CreateTable(
                name: "PricingRule",
                columns: table => new
                {
                    PricingRuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RatePerKwh = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConnectorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StationId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRule", x => x.PricingRuleId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingRule_StationId",
                table: "PricingRule",
                column: "StationId");

            // Booking: Thêm cột Price
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Booking",
                type: "decimal(12,2)",
                nullable: true);

            // ChargingSession: Thêm cột PricingRuleId, Subtotal, Tax, Total, DurationMin, IdleMin, StartSoc, EndSoc, EnergyKwh
            migrationBuilder.AddColumn<int>(
                name: "PricingRuleId",
                table: "ChargingSession",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "ChargingSession",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "ChargingSession",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "ChargingSession",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMin",
                table: "ChargingSession",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdleMin",
                table: "ChargingSession",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartSoc",
                table: "ChargingSession",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EndSoc",
                table: "ChargingSession",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EnergyKwh",
                table: "ChargingSession",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSession_PricingRuleId",
                table: "ChargingSession",
                column: "PricingRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingSession_PricingRule",
                table: "ChargingSession",
                column: "PricingRuleId",
                principalTable: "PricingRule",
                principalColumn: "PricingRuleId",
                onDelete: ReferentialAction.Restrict);

            // Invoice: Thêm cột Subtotal, SubscriptionAdjustment, Tax, Total
            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Invoice",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubscriptionAdjustment",
                table: "Invoice",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "Invoice",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Invoice",
                type: "decimal(12,2)",
                nullable: true);

            // Payment: Thêm cột Method, Amount, PaidAt
            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "Payment",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Payment",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Payment",
                type: "datetime",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ChargingSession_PricingRule", table: "ChargingSession");
            migrationBuilder.DropIndex(name: "IX_ChargingSession_PricingRuleId", table: "ChargingSession");

            migrationBuilder.DropColumn(name: "Price", table: "Booking");

            migrationBuilder.DropColumn(name: "PricingRuleId", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "Subtotal", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "Tax", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "Total", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "DurationMin", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "IdleMin", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "StartSoc", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "EndSoc", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "EnergyKwh", table: "ChargingSession");

            migrationBuilder.DropColumn(name: "Subtotal", table: "Invoice");
            migrationBuilder.DropColumn(name: "SubscriptionAdjustment", table: "Invoice");
            migrationBuilder.DropColumn(name: "Tax", table: "Invoice");
            migrationBuilder.DropColumn(name: "Total", table: "Invoice");

            migrationBuilder.DropColumn(name: "Method", table: "Payment");
            migrationBuilder.DropColumn(name: "Amount", table: "Payment");
            migrationBuilder.DropColumn(name: "PaidAt", table: "Payment");

            migrationBuilder.DropTable(name: "PricingRule");
        }
    }
}
