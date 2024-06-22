using Microsoft.EntityFrameworkCore.Migrations;
using product_scraper.Repositories;
using System;
using System.Net.WebSockets;

#nullable disable

namespace product_scraper.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MercariListings_UrlHash",
                table: "MercariListings",
                column: "UrlHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
