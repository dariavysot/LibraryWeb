using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Створюємо таблицю MembershipTypes
            migrationBuilder.CreateTable(
                name: "MembershipTypes",
                columns: table => new
                {
                    MembershipTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipTypes", x => x.MembershipTypeID);
                });

            // 2. Вставка запису Standard
            migrationBuilder.InsertData(
                table: "MembershipTypes",
                columns: new[] { "Name", "Price", "DurationMonths" },
                values: new object[] { "Standard", 100m, 1 });

            // 3. Додаємо колонку MembershipTypeID у Memberships
            migrationBuilder.AddColumn<int>(
                name: "MembershipTypeID",
                table: "Memberships",
                nullable: false,
                defaultValue: 1);

            // 4. Створюємо FK
            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MembershipTypeID",
                table: "Memberships",
                column: "MembershipTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_MembershipTypes_MembershipTypeID",
                table: "Memberships",
                column: "MembershipTypeID",
                principalTable: "MembershipTypes",
                principalColumn: "MembershipTypeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_MembershipTypes_MembershipTypeID",
                table: "Memberships");

            migrationBuilder.DropTable(
                name: "MembershipTypes");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_MembershipTypeID",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "MembershipTypeID",
                table: "Memberships");
        }
    }
}
