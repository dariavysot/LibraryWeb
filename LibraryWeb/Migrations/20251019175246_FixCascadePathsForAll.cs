using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePathsForAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_EmployeeID",
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
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_EmployeeID",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "UserPassword",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_ReaderID",
                table: "Memberships",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Memberships_MembershipID",
                table: "Payments",
                column: "MembershipID",
                principalTable: "Memberships",
                principalColumn: "MembershipID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum",
                onDelete: ReferentialAction.Cascade);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Users_EmployeeID",
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
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_EmployeeID",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "UserPassword",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Users",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Copies_InventoryNum",
                table: "Loans",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Users_EmployeeID",
                table: "Loans",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID");

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
                name: "FK_Reservations_Copies_InventoryNum",
                table: "Reservations",
                column: "InventoryNum",
                principalTable: "Copies",
                principalColumn: "InventoryNum");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_EmployeeID",
                table: "Reservations",
                column: "EmployeeID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Users_ReaderID",
                table: "Reservations",
                column: "ReaderID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
