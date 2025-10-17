using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    public partial class FixInvoiceChargingSessionRelation_Clean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==============================
            // 1️⃣ Thêm cột InvoiceId cho ChargingSession
            // ==============================
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "ChargingSession",
                type: "int",
                nullable: true);

            // ==============================
            // 2️⃣ Tạo bảng SubscriptionPlan (nếu chưa có)
            // ==============================
            migrationBuilder.CreateTable(
                name: "SubscriptionPlan",
                columns: table => new
                {
                    SubscriptionPlanId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    Category = table.Column<string>(maxLength: 50, nullable: false),
                    PriceMonthly = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PriceYearly = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FreeIdleMinutes = table.Column<int>(nullable: true),
                    Benefits = table.Column<string>(maxLength: 500, nullable: false),
                    IsForCompany = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlan", x => x.SubscriptionPlanId);
                });

            // ==============================
            // 3️⃣ Tạo bảng Subscription
            // ==============================
            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionPlanId = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: true),
                    CompanyId = table.Column<int>(nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    BillingCycle = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "Monthly"),
                    AutoRenew = table.Column<bool>(nullable: false, defaultValue: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscription_SubscriptionPlan",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlan",
                        principalColumn: "SubscriptionPlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscription_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_Subscription_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_SubscriptionPlanId",
                table: "Subscription",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_CustomerId",
                table: "Subscription",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_CompanyId",
                table: "Subscription",
                column: "CompanyId");

            // ==============================
            // 4️⃣ Tạo quan hệ Invoice – Subscription (n:1)
            // ==============================
            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Subscription",
                table: "Invoice",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "SubscriptionId",
                onDelete: ReferentialAction.Restrict);

            // ==============================
            // 5️⃣ Tạo quan hệ ChargingSession – Invoice (n:1)
            // ==============================
            migrationBuilder.CreateIndex(
                name: "IX_ChargingSession_InvoiceId",
                table: "ChargingSession",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingSession_Invoice",
                table: "ChargingSession",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa FK và index trước khi drop table
            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSession_Invoice",
                table: "ChargingSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Subscription",
                table: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_ChargingSession_InvoiceId",
                table: "ChargingSession");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "ChargingSession");

            migrationBuilder.DropTable(name: "Subscription");
            migrationBuilder.DropTable(name: "SubscriptionPlan");
        }
    }
}
