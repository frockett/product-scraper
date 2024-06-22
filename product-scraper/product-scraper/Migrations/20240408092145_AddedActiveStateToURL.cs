using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace product_scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddedActiveStateToURL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Urls",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Urls");
        }
    }
}
