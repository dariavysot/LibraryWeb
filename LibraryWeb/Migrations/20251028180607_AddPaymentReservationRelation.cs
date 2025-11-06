using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReservationRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Memberships_MembershipID",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "PaymentID",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MembershipID",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ReservationID",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PaymentID",
                table: "Reservations",
                column: "PaymentID",
                unique: true,
                filter: "[PaymentID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Memberships_MembershipID",
                table: "Payments",
                column: "MembershipID",
                principalTable: "Memberships",
                principalColumn: "MembershipID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Payments_PaymentID",
                table: "Reservations",
                column: "PaymentID",
                principalTable: "Payments",
                principalColumn: "PaymentID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Memberships_MembershipID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Payments_PaymentID",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_PaymentID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReservationID",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "MembershipID",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Memberships_MembershipID",
                table: "Payments",
                column: "MembershipID",
                principalTable: "Memberships",
                principalColumn: "MembershipID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
