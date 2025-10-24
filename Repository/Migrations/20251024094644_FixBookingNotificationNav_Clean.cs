using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    public partial class FixBookingNotificationNav_Clean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔹 Thêm liên kết giữa Notification và Booking
            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Booking_BookingId",
                table: "Notifications",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.SetNull);

            // 🔹 Thêm liên kết giữa Notification và Invoice
            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Invoice_InvoiceId",
                table: "Notifications",
                column: "InvoiceId",
                principalTable: "Invoice",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.SetNull);

            // 🔹 Thêm liên kết giữa Notification và Subscription
            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Subscription_SubscriptionId",
                table: "Notifications",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "SubscriptionId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 🔹 Gỡ bỏ các liên kết nếu rollback
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Booking_BookingId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Invoice_InvoiceId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Subscription_SubscriptionId",
                table: "Notifications");
        }
    }
}
