using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoanID",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_LoanID",
                table: "Payments",
                column: "LoanID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Loans_LoanID",
                table: "Payments",
                column: "LoanID",
                principalTable: "Loans",
                principalColumn: "LoanID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Loans_LoanID",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_LoanID",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LoanID",
                table: "Payments");
        }
    }
}
