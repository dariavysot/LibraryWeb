using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_ReaderID",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations");

            migrationBuilder.AddColumn<int>(
                name: "MembershipID",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MembershipID",
                table: "Payments",
                column: "MembershipID");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_ReaderID",
                table: "Loans",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Memberships_MembershipID",
                table: "Payments",
                column: "MembershipID",
                principalTable: "Memberships",
                principalColumn: "MembershipID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_ReaderID",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Memberships_MembershipID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Payments_MembershipID",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MembershipID",
                table: "Payments");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_ReaderID",
                table: "Loans",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
