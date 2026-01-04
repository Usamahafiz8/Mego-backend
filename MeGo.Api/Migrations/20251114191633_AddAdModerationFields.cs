using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeGo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAdModerationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Ads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Ads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "Ads",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "Ads");
        }
    }
}
