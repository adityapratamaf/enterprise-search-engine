using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A key column must be a bounded type. The model already declares
            // Name as nvarchar(200), but databases where the earlier
            // AddSearchEngineConstraints migration never ran (it ships without a
            // Designer/[Migration] attribute, so EF skips it) still have Name
            // as nvarchar(max). Align the column to the model before indexing;
            // this is effectively a no-op where Name is already nvarchar(200).
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                table: "Products");
        }
    }
}
