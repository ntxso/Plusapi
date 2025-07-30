using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixStockTableChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks");

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Stocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Stocks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "PhoneModelId",
                table: "Stocks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ColorId",
                table: "Stocks",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_PhoneModelId",
                table: "Stocks",
                column: "PhoneModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductId_PhoneModelId_ColorId",
                table: "Stocks",
                columns: new[] { "ProductId", "PhoneModelId", "ColorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Colors_ColorId",
                table: "Stocks",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_PhoneModels_PhoneModelId",
                table: "Stocks",
                column: "PhoneModelId",
                principalTable: "PhoneModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Colors_ColorId",
                table: "Stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_PhoneModels_PhoneModelId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_ColorId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_PhoneModelId",
                table: "Stocks");

            migrationBuilder.DropIndex(
                name: "IX_Stocks_ProductId_PhoneModelId_ColorId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "PhoneModelId",
                table: "Stocks");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductId",
                table: "Stocks",
                column: "ProductId",
                unique: true);
        }
    }
}
