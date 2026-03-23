using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOS100_LoanAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddItemNameToLoanUserItemStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "LoanUserItemStats",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "LoanUserItemStats");
        }
    }
}
