using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReservationPaymentRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Payments_PaymentID",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PaymentID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReservationID",
                table: "Payments",
                column: "ReservationID",
                unique: true,
                filter: "[ReservationID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Reservations_ReservationID",
                table: "Payments",
                column: "ReservationID",
                principalTable: "Reservations",
                principalColumn: "ReservationID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Reservations_ReservationID",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ReservationID",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "PaymentID",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PaymentID",
                table: "Reservations",
                column: "PaymentID",
                unique: true,
                filter: "[PaymentID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Payments_PaymentID",
                table: "Reservations",
                column: "PaymentID",
                principalTable: "Payments",
                principalColumn: "PaymentID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
