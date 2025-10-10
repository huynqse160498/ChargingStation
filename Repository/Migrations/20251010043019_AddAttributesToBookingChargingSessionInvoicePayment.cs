using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    public partial class AddAttributesToBookingChargingSessionInvoicePayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Booking
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Booking",
                type: "decimal(12,2)",
                nullable: true);

         

            migrationBuilder.AddColumn<int>(
                name: "EndSoc",
                table: "ChargingSession",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EnergyKwh",
                table: "ChargingSession",
                type: "decimal(12,2)",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "ChargingSession",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndedAt",
                table: "ChargingSession",
                type: "datetime",
                nullable: true);

            // Invoice
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

            // Payment
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

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Payment",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Success");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Booking
            migrationBuilder.DropColumn(name: "Price", table: "Booking");

            // ChargingSession
            migrationBuilder.DropColumn(name: "EndSoc", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "EnergyKwh", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "Subtotal", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "Tax", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "Total", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "DurationMin", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "IdleMin", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "StartedAt", table: "ChargingSession");
            migrationBuilder.DropColumn(name: "EndedAt", table: "ChargingSession");

            // Invoice
            migrationBuilder.DropColumn(name: "Subtotal", table: "Invoice");
            migrationBuilder.DropColumn(name: "SubscriptionAdjustment", table: "Invoice");
            migrationBuilder.DropColumn(name: "Tax", table: "Invoice");
            migrationBuilder.DropColumn(name: "Total", table: "Invoice");

            // Payment
            migrationBuilder.DropColumn(name: "Method", table: "Payment");
            migrationBuilder.DropColumn(name: "Amount", table: "Payment");
            migrationBuilder.DropColumn(name: "PaidAt", table: "Payment");
            migrationBuilder.DropColumn(name: "Status", table: "Payment");
        }
    }
}
