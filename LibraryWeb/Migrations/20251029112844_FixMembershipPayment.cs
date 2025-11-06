using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixMembershipPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_MembershipID",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MembershipID",
                table: "Payments",
                column: "MembershipID",
                unique: true,
                filter: "[MembershipID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_MembershipID",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MembershipID",
                table: "Payments",
                column: "MembershipID");
        }
    }
}
