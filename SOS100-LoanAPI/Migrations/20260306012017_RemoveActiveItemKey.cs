using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOS100_LoanAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveActiveItemKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveItemKey",
                table: "Loans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveItemKey",
                table: "Loans",
                type: "INTEGER",
                nullable: true);
        }
    }
}
