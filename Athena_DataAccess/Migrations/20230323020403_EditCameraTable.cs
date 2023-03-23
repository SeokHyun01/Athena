using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AthenaDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EditCameraTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_AspNetUsers_UserId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_FCMInfos_AspNetUsers_UserId",
                table: "FCMInfos");

            migrationBuilder.DropIndex(
                name: "IX_FCMInfos_UserId",
                table: "FCMInfos");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_UserId",
                table: "Cameras");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FCMInfos",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "FCMInfos",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Cameras",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FCMInfos_AppUserId",
                table: "FCMInfos",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FCMInfos_AspNetUsers_AppUserId",
                table: "FCMInfos",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FCMInfos_AspNetUsers_AppUserId",
                table: "FCMInfos");

            migrationBuilder.DropIndex(
                name: "IX_FCMInfos_AppUserId",
                table: "FCMInfos");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "FCMInfos");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FCMInfos",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Cameras",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FCMInfos_UserId",
                table: "FCMInfos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_UserId",
                table: "Cameras",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_AspNetUsers_UserId",
                table: "Cameras",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FCMInfos_AspNetUsers_UserId",
                table: "FCMInfos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
