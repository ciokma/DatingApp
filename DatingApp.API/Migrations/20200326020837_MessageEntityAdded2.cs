using Microsoft.EntityFrameworkCore.Migrations;

namespace DatingApp.API.Migrations
{
    public partial class MessageEntityAdded2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Messages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Messages");
        }
    }
}
