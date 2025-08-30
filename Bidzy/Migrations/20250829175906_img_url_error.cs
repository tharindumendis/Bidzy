using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bidzy.Migrations
{
    /// <inheritdoc />
    public partial class img_url_error : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Auctions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Auctions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
