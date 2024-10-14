using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.NET_Project.Migrations
{
    /// <inheritdoc />
    public partial class ambotaniuiwd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
