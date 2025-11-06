using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoanUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_EmployeeID",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_ReaderID",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_EmployeeID",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "EmployeeID",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "ReaderID",
                table: "Loans",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_ReaderID",
                table: "Loans",
                newName: "IX_Loans_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_UserID",
                table: "Loans",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_UserID",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Loans",
                newName: "ReaderID");

            migrationBuilder.RenameIndex(
                name: "IX_Loans_UserID",
                table: "Loans",
                newName: "IX_Loans_ReaderID");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeID",
                table: "Loans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_EmployeeID",
                table: "Loans",
                column: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_EmployeeID",
                table: "Loans",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_ReaderID",
                table: "Loans",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
