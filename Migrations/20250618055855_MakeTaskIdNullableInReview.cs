using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wordwave.Migrations
{
    /// <inheritdoc />
    public partial class MakeTaskIdNullableInReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Tasks_TaskId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Reviews",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Tasks_TaskId",
                table: "Reviews",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Tasks_TaskId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Tasks_TaskId",
                table: "Reviews",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
