using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddQAAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToQAUserId",
                table: "Tasks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToQAUserId",
                table: "Tasks",
                column: "AssignedToQAUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_AssignedToQAUserId",
                table: "Tasks",
                column: "AssignedToQAUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_AssignedToQAUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_AssignedToQAUserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AssignedToQAUserId",
                table: "Tasks");
        }
    }
}
