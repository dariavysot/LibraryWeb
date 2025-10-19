using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships",
                column: "ReaderID",
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
        }
    }
}
