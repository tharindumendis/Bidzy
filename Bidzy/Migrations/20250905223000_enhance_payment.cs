using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bidzy.Migrations
{
    public partial class enhance_payment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountCaptured",
                table: "Payments",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChargeId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "Payments",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProcessorFee",
                table: "Payments",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptUrl",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentIntentId",
                table: "Payments",
                column: "PaymentIntentId",
                unique: true,
                filter: "[PaymentIntentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ChargeId",
                table: "Payments",
                column: "ChargeId",
                unique: true,
                filter: "[ChargeId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentIntentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ChargeId",
                table: "Payments");

            migrationBuilder.DropColumn(name: "AmountCaptured", table: "Payments");
            migrationBuilder.DropColumn(name: "ChargeId", table: "Payments");
            migrationBuilder.DropColumn(name: "Currency", table: "Payments");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Payments");
            migrationBuilder.DropColumn(name: "NetAmount", table: "Payments");
            migrationBuilder.DropColumn(name: "PaymentIntentId", table: "Payments");
            migrationBuilder.DropColumn(name: "ProcessorFee", table: "Payments");
            migrationBuilder.DropColumn(name: "ReceiptUrl", table: "Payments");
            migrationBuilder.DropColumn(name: "StatusReason", table: "Payments");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Payments");
        }
    }
}

