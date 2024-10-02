using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.NET_Project.Migrations
{
    /// <inheritdoc />
    public partial class ProgressAdjusted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Progresses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "Progresses",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Progresses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Progresses");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "Progresses");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Progresses");
        }
    }
}
