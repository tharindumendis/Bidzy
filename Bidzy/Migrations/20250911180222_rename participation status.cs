using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bidzy.Migrations
{
    /// <inheritdoc />
    public partial class renameparticipationstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "participationStatus",
                table: "AuctionParticipations",
                newName: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "AuctionParticipations",
                newName: "participationStatus");
        }
    }
}
