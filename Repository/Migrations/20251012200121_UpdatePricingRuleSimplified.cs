using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePricingRuleSimplified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PricingRule_Station",
                table: "PricingRule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PricingRule",
                table: "PricingRule");

            migrationBuilder.DropColumn(
                name: "ConnectorType",
                table: "PricingRule");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "PricingRule");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "PricingRule");

            migrationBuilder.DropColumn(
                name: "RatePerKwh",
                table: "PricingRule");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PricingRule");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "PricingRule");

            migrationBuilder.RenameTable(
                name: "PricingRule",
                newName: "PricingRules");

            migrationBuilder.RenameIndex(
                name: "IX_PricingRule_StationId",
                table: "PricingRules",
                newName: "IX_PricingRules_StationId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PricingRules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PricingRules",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "Active");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PricingRules",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<decimal>(
                name: "IdleFeePerMin",
                table: "PricingRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerKwh",
                table: "PricingRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TimeRange",
                table: "PricingRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "PricingRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricingRules",
                table: "PricingRules",
                column: "PricingRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PricingRules_Station_StationId",
                table: "PricingRules",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PricingRules_Station_StationId",
                table: "PricingRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PricingRules",
                table: "PricingRules");

            migrationBuilder.DropColumn(
                name: "IdleFeePerMin",
                table: "PricingRules");

            migrationBuilder.DropColumn(
                name: "PricePerKwh",
                table: "PricingRules");

            migrationBuilder.DropColumn(
                name: "TimeRange",
                table: "PricingRules");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "PricingRules");

            migrationBuilder.RenameTable(
                name: "PricingRules",
                newName: "PricingRule");

            migrationBuilder.RenameIndex(
                name: "IX_PricingRules_StationId",
                table: "PricingRule",
                newName: "IX_PricingRule_StationId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PricingRule",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PricingRule",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Active",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PricingRule",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "ConnectorType",
                table: "PricingRule",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "PricingRule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PricingRule",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RatePerKwh",
                table: "PricingRule",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PricingRule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "PricingRule",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PricingRule",
                table: "PricingRule",
                column: "PricingRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PricingRule_Station",
                table: "PricingRule",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "StationId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
