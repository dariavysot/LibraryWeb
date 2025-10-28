using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentToMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentID",
                table: "Memberships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentID1",
                table: "Memberships",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_PaymentID1",
                table: "Memberships",
                column: "PaymentID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Payments_PaymentID1",
                table: "Memberships",
                column: "PaymentID1",
                principalTable: "Payments",
                principalColumn: "PaymentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Payments_PaymentID1",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_PaymentID1",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "PaymentID1",
                table: "Memberships");
        }
    }
}
