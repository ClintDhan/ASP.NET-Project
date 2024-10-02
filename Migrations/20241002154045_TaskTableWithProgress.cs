using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.NET_Project.Migrations
{
    /// <inheritdoc />
    public partial class TaskTableWithProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Projects_ProjectId",
                table: "Progresses");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_ProjectId",
                table: "Progresses");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Progresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Progresses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_ProjectId",
                table: "Progresses",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Projects_ProjectId",
                table: "Progresses",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}
