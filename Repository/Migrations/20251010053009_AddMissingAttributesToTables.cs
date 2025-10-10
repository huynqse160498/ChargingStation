using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    public partial class AddMissingAttributesToTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Booking.Price
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Booking",
                type: "decimal(18,2)",
                nullable: true);

          

       

      

            // ChargingSession.Subtotal
            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "ChargingSession",
                type: "decimal(18,2)",
                nullable: true);

    

          

           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa FK
            migrationBuilder.DropForeignKey(name: "FK_Payment_Booking", table: "Payment");

            // Xóa index
            migrationBuilder.DropIndex(name: "IX_Payment_BookingId", table: "Payment");

            // Xóa cột
            migrationBuilder.DropColumn(name: "Price", table: "Booking");
            migrationBuilder.DropColumn(name: "BookingId", table: "Payment");
            migrationBuilder.DropColumn(name: "Subtotal", table: "ChargingSession");
        }
    }
}
