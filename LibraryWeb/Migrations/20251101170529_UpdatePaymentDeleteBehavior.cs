using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_UserID",
                table: "Memberships",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserID",
                table: "Payments",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
