using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bidzy.Migrations
{
    /// <inheritdoc />
    public partial class tagToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Tag_TagstagId",
                table: "ProductTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "tagId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Tags_TagstagId",
                table: "ProductTag",
                column: "TagstagId",
                principalTable: "Tags",
                principalColumn: "tagId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Tags_TagstagId",
                table: "ProductTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "tagId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Tag_TagstagId",
                table: "ProductTag",
                column: "TagstagId",
                principalTable: "Tag",
                principalColumn: "tagId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
