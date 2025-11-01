using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoanDeletionBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_UserID",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Loans",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_UserID",
                table: "Loans",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_UserID",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_UserID",
                table: "Loans",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
