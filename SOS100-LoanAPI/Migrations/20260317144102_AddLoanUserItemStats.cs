using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOS100_LoanAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanUserItemStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoanUserItemStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BorrowerId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLoans = table.Column<int>(type: "INTEGER", nullable: false),
                    LateReturns = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanUserItemStats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanUserItemStats_BorrowerId_ItemId",
                table: "LoanUserItemStats",
                columns: new[] { "BorrowerId", "ItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanUserItemStats");
        }
    }
}
