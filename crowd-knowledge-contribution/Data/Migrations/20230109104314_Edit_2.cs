using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crowd_knowledge_contribution.Data.Migrations
{
    public partial class AddMigrationtar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Edits",
                newName: "tId");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Edits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Edits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Edits");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Edits");

            migrationBuilder.RenameColumn(
                name: "tId",
                table: "Edits",
                newName: "Id");
        }
    }
}
