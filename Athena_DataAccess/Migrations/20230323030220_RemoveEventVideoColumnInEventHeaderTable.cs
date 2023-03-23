using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AthenaDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEventVideoColumnInEventHeaderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventHeaders_EventVideos_EventVideoId",
                table: "EventHeaders");

            migrationBuilder.DropIndex(
                name: "IX_EventHeaders_EventVideoId",
                table: "EventHeaders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EventHeaders_EventVideoId",
                table: "EventHeaders",
                column: "EventVideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventHeaders_EventVideos_EventVideoId",
                table: "EventHeaders",
                column: "EventVideoId",
                principalTable: "EventVideos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
