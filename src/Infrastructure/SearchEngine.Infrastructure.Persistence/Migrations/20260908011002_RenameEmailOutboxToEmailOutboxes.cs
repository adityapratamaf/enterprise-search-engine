using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmailOutboxToEmailOutboxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailOutbox",
                table: "EmailOutbox");

            migrationBuilder.RenameTable(
                name: "EmailOutbox",
                newName: "EmailOutboxes");

            migrationBuilder.RenameIndex(
                name: "IX_EmailOutbox_Status",
                table: "EmailOutboxes",
                newName: "IX_EmailOutboxes_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailOutboxes",
                table: "EmailOutboxes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailOutboxes",
                table: "EmailOutboxes");

            migrationBuilder.RenameTable(
                name: "EmailOutboxes",
                newName: "EmailOutbox");

            migrationBuilder.RenameIndex(
                name: "IX_EmailOutboxes_Status",
                table: "EmailOutbox",
                newName: "IX_EmailOutbox_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailOutbox",
                table: "EmailOutbox",
                column: "Id");
        }
    }
}
