using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoInventory.Server.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCaseInsensitiveSkuUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Products_Sku\";");
            migrationBuilder.Sql("CREATE UNIQUE INDEX \"IX_Products_Sku\" ON \"Products\" (\"Sku\" COLLATE NOCASE);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Products_Sku\";");
            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);
        }
    }
}
