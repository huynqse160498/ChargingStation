using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelWithDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Customer",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Port",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Vehicle",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Charger_Station",
                table: "Charger");



            migrationBuilder.DropForeignKey(
                name: "FK_Session_Booking",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Customer",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Port",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Vehicle",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Account",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Company",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Session",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Subscriptions_SubscriptionId",
                table: "Invoice");



            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Customer",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Invoice",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Port_Charger",
                table: "Port");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingRules_Station_StationId",
                table: "PricingRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Company",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Customer",
                table: "Vehicle");

        }
          

           

      

           


        


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Customer_CustomerId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Port_PortId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Vehicle_VehicleId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Charger_Station_StationId",
                table: "Charger");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_Booking_BookingId",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_Customer_CustomerId",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_Invoice",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_Port_PortId",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_PricingRule_PricingRuleId",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_Vehicle_VehicleId",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Account_AccountId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Company_CompanyId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Customer_CustomerId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Subscription",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Booking_BookingId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Customer_CustomerId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Invoice_InvoiceId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Port_Charger_ChargerId",
                table: "Port");

            migrationBuilder.DropForeignKey(
                name: "FK_PricingRule_Station_StationId",
                table: "PricingRule");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Company_CompanyId",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_Customer_CustomerId",
                table: "Vehicle");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vehicle",
                table: "Vehicle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Station",
                table: "Station");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Port",
                table: "Port");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invoice",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_CustomerId",
                table: "Invoice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customer",
                table: "Customer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Company",
                table: "Company");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChargingSession",
                table: "ChargingSession");

            migrationBuilder.DropIndex(
                name: "IX_ChargingSession_InvoiceId",
                table: "ChargingSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Charger",
                table: "Charger");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Account",
                table: "Account");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlan",
                table: "SubscriptionPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PricingRule",
                table: "PricingRule");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "ChargingSession");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlan",
                newName: "SubscriptionPlans");

            migrationBuilder.RenameTable(
                name: "PricingRule",
                newName: "PricingRules");

            migrationBuilder.RenameColumn(
                name: "MaxPowerKw",
                table: "Port",
                newName: "MaxPowerKW");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Invoice",
                newName: "ChargingSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_ChargingSession_BookingId",
                table: "ChargingSession",
                newName: "UQ__Charging__BookingId");

            migrationBuilder.RenameColumn(
                name: "PowerKw",
                table: "Charger",
                newName: "PowerKW");

            migrationBuilder.RenameIndex(
                name: "IX_PricingRule_StationId",
                table: "PricingRules",
                newName: "IX_PricingRules_StationId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Vehicle",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Vehicle",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Vehicle",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "Vehicle",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Vehicle",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Vehicle",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConnectorType",
                table: "Vehicle",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CarMaker",
                table: "Vehicle",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BatteryCapacity",
                table: "Vehicle",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Station",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Station",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StationName",
                table: "Station",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Station",
                type: "decimal(9,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Station",
                type: "decimal(9,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Station",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Station",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Station",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Station",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Port",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Port",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxPowerKW",
                table: "Port",
                type: "decimal(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Port",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Port",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConnectorType",
                table: "Port",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Port",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Payment",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Payment",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Success",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidAt",
                table: "Payment",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Payment",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Payment",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payment",
                type: "decimal(12,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Invoice",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoice",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<int>(
                name: "BillingYear",
                table: "Invoice",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BillingMonth",
                table: "Invoice",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "InstalledAt",
                table: "Charger",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceYearly",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceMonthly",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AlterColumn<string>(
                name: "PlanName",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercent",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Benefits",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Vehicle__476B5492D9D26F86",
                table: "Vehicle",
                column: "VehicleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Station__E0D8A6BD39F2BE04",
                table: "Station",
                column: "StationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Port__D859BF8F9FD220FA",
                table: "Port",
                column: "PortId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Payment__9B556A38F8427D9A",
                table: "Payment",
                column: "PaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Invoice__D796AAB53C796279",
                table: "Invoice",
                column: "InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Customer__A4AE64D85719FCC9",
                table: "Customer",
                column: "CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Company__2D971CACA4C945E6",
                table: "Company",
                column: "CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Charging__F980BBA01825010D",
                table: "ChargingSession",
                column: "ChargingSessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Charger__EE76D991806955FB",
                table: "Charger",
                column: "ChargerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Booking__73951AED49396CC4",
                table: "Booking",
                column: "BookingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Account__349DA5A68BF58D4A",
                table: "Account",
                column: "AccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans",
                column: "SubscriptionPlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricingRules",
                table: "PricingRules",
                column: "PricingRuleId");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    SubscriptionPlanId = table.Column<int>(type: "int", nullable: false),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    BillingCycle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId");
                    table.ForeignKey(
                        name: "FK_Subscriptions_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "SubscriptionPlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Invoice__ChargingSessionId",
                table: "Invoice",
                column: "ChargingSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CompanyId",
                table: "Subscriptions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CustomerId",
                table: "Subscriptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionPlanId",
                table: "Subscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Customer",
                table: "Booking",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Port",
                table: "Booking",
                column: "PortId",
                principalTable: "Port",
                principalColumn: "PortId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Vehicle",
                table: "Booking",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Charger_Station",
                table: "Charger",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingSession_PricingRule",
                table: "ChargingSession",
                column: "PricingRuleId",
                principalTable: "PricingRules",
                principalColumn: "PricingRuleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Booking",
                table: "ChargingSession",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Customer",
                table: "ChargingSession",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Port",
                table: "ChargingSession",
                column: "PortId",
                principalTable: "Port",
                principalColumn: "PortId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Vehicle",
                table: "ChargingSession",
                column: "VehicleId",
                principalTable: "Vehicle",
                principalColumn: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Account",
                table: "Customer",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Company",
                table: "Customer",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Session",
                table: "Invoice",
                column: "ChargingSessionId",
                principalTable: "ChargingSession",
                principalColumn: "ChargingSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Subscriptions_SubscriptionId",
                table: "Invoice",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Booking",
                table: "Payment",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Customer",
                table: "Payment",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Invoice",
                table: "Payment",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Port_Charger",
                table: "Port",
                column: "ChargerId",
                principalTable: "Charger",
                principalColumn: "ChargerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PricingRules_Station_StationId",
                table: "PricingRules",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Company",
                table: "Vehicle",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_Customer",
                table: "Vehicle",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId");
        }
    }
}
