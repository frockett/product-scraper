using Microsoft.EntityFrameworkCore.Migrations;
using product_scraper.Repositories;

#nullable disable

namespace product_scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlHash1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlHash",
                table: "MercariListings",
                type: "TEXT",
                nullable: true);

            /*
            migrationBuilder.CreateIndex(
                name: "IX_MercariListings_UrlHash",
                table: "MercariListings",
                column: "UrlHash");
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlHash",
                table: "MercariListings");
        }
    }
}
