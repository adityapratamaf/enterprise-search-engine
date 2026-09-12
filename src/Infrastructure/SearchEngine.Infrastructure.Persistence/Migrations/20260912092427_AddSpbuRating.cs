using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpbuRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JumlahUlasan",
                table: "Spbus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Spbus",
                type: "decimal(2,1)",
                precision: 2,
                scale: 1,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spbus_Rating",
                table: "Spbus",
                column: "Rating",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Spbus_Rating",
                table: "Spbus");

            migrationBuilder.DropColumn(
                name: "JumlahUlasan",
                table: "Spbus");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Spbus");
        }
    }
}
