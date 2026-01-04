using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeGo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddKycInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_KycInfo_KycInfoId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_KycInfoId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KycInfo",
                table: "KycInfo");

            migrationBuilder.DropColumn(
                name: "KycInfoId",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "KycInfo",
                newName: "KycInfos");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "KycInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "KycInfos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "KycInfos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "KycInfos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_KycInfos",
                table: "KycInfos",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_KycInfos_UserId",
                table: "KycInfos",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KycInfos_UserId1",
                table: "KycInfos",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_KycInfos_Users_UserId",
                table: "KycInfos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KycInfos_Users_UserId1",
                table: "KycInfos",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KycInfos_Users_UserId",
                table: "KycInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_KycInfos_Users_UserId1",
                table: "KycInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KycInfos",
                table: "KycInfos");

            migrationBuilder.DropIndex(
                name: "IX_KycInfos_UserId",
                table: "KycInfos");

            migrationBuilder.DropIndex(
                name: "IX_KycInfos_UserId1",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "KycInfos");

            migrationBuilder.RenameTable(
                name: "KycInfos",
                newName: "KycInfo");

            migrationBuilder.AddColumn<Guid>(
                name: "KycInfoId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_KycInfo",
                table: "KycInfo",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_KycInfoId",
                table: "Users",
                column: "KycInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_KycInfo_KycInfoId",
                table: "Users",
                column: "KycInfoId",
                principalTable: "KycInfo",
                principalColumn: "Id");
        }
    }
}
