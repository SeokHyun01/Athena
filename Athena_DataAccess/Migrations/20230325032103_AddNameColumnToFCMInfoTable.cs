using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AthenaDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNameColumnToFCMInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FCMInfos",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "FCMInfos");
        }
    }
}
