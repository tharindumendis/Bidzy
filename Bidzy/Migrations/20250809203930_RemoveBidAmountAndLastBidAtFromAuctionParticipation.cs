using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bidzy.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBidAmountAndLastBidAtFromAuctionParticipation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuctionParticipations",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    // store enum as int (default). If you used .HasConversion<string>(), change type to nvarchar(max)
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionParticipations", x => new { x.UserId, x.AuctionId });

                    table.ForeignKey(
                        name: "FK_AuctionParticipations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction); // matches .OnDelete(DeleteBehavior.NoAction)

                    table.ForeignKey(
                        name: "FK_AuctionParticipations_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade); // matches .OnDelete(DeleteBehavior.Cascade)
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuctionParticipations_AuctionId",
                table: "AuctionParticipations",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionParticipations_UserId_Status",
                table: "AuctionParticipations",
                columns: new[] { "UserId", "Status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuctionParticipations");
        }
    }
}
