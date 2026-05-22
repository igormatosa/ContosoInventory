using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoInventory.Server.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCaseInsensitiveCategoryNameUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Categories_Name\";");
            migrationBuilder.Sql("CREATE UNIQUE INDEX \"IX_Categories_Name\" ON \"Categories\" (\"Name\" COLLATE NOCASE);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Categories_Name\";");
            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }
    }
}
