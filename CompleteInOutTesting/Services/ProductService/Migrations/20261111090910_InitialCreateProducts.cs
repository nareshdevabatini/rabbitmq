using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpirationDate = table.Column<string>(type: "TEXT", nullable: false),
                    ProductAvailability = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "ExpirationDate", "Name", "Price", "ProductAvailability" },
                values: new object[,]
                {
                    { 1, "Product 1 description", "2024-12-31", "Product 1", 10.0m, true },
                    { 2, "Product 2 description", "2024-12-31", "Product 2", 20.0m, true },
                    { 3, "Product 3 description", "2024-12-31", "Product 3", 30.0m, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
