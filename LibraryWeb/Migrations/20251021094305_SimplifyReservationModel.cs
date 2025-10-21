using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyReservationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_EmployeeID",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EmployeeID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "EmployeeID",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ReaderID",
                table: "Reservations",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_ReaderID",
                table: "Reservations",
                newName: "IX_Reservations_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_UserID",
                table: "Reservations",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_UserID",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Reservations",
                newName: "ReaderID");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_UserID",
                table: "Reservations",
                newName: "IX_Reservations_ReaderID");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeID",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EmployeeID",
                table: "Reservations",
                column: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_EmployeeID",
                table: "Reservations",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
