using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMembershipToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships");

            migrationBuilder.RenameColumn(
                name: "ReaderID",
                table: "Memberships",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_ReaderID",
                table: "Memberships",
                newName: "IX_Memberships_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Memberships",
                newName: "ReaderID");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_UserID",
                table: "Memberships",
                newName: "IX_Memberships_ReaderID");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
