using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace product_scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddIsEmailedToMercariListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "imgUrl",
                table: "MercariListings",
                newName: "ImgUrl");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailed",
                table: "MercariListings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailed",
                table: "MercariListings");

            migrationBuilder.RenameColumn(
                name: "ImgUrl",
                table: "MercariListings",
                newName: "imgUrl");
        }
    }
}
