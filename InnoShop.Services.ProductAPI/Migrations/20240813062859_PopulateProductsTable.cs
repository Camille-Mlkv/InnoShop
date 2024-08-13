using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnoShop.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class PopulateProductsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Date", "Description", "IsAvailable", "Name", "Price", "UserId" },
                values: new object[] { 1, new DateTime(2024, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Extremely delicious pie", true, "Apple pie", 25.0, "616e56f5-3406-4178-8691-b4e6325c8a37" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);
        }
    }
}
