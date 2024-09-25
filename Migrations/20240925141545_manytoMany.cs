using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.NET_Project.Migrations
{
    /// <inheritdoc />
    public partial class manytoMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Projects_ProjectId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProjectId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserProjects",
                columns: table => new
                {
                    ProjectsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjects", x => new { x.ProjectsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UserProjects_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjects_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UsersId",
                table: "UserProjects",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProjects");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProjectId",
                table: "Users",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Projects_ProjectId",
                table: "Users",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
