using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeGo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFavoriteReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_Ads_AdId",
                table: "Media");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Ads_AdId",
                table: "Media",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_Ads_AdId",
                table: "Media");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Ads_AdId",
                table: "Media",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id");
        }
    }
}
