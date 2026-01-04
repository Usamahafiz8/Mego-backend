using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MeGo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoHiddenAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LiveVerificationScheduledAt",
                table: "KycInfos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiveVerificationSessionId",
                table: "KycInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiveVerificationVideoUrl",
                table: "KycInfos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationTier",
                table: "KycInfos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AutoHiddenAt",
                table: "Ads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BoostedUntil",
                table: "Ads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                table: "Ads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FraudReportCount",
                table: "Ads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBoosted",
                table: "Ads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFraud",
                table: "Ads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpam",
                table: "Ads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRepostedAt",
                table: "Ads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaveCount",
                table: "Ads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShareCount",
                table: "Ads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpamReportCount",
                table: "Ads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Ads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VoiceDescriptionUrl",
                table: "Ads",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdId = table.Column<int>(type: "integer", nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    Saves = table.Column<int>(type: "integer", nullable: false),
                    Shares = table.Column<int>(type: "integer", nullable: false),
                    Messages = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdAnalytics_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    PreviousValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdHistories_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdQualityScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdId = table.Column<int>(type: "integer", nullable: false),
                    TitleScore = table.Column<int>(type: "integer", nullable: false),
                    ImageScore = table.Column<int>(type: "integer", nullable: false),
                    DescriptionScore = table.Column<int>(type: "integer", nullable: false),
                    CompletenessScore = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdQualityScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdQualityScores_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdRelistReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReminderSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    InactiveDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdRelistReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdRelistReminders_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdRelistReminders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdShareTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateName = table.Column<string>(type: "text", nullable: false),
                    TemplateType = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    HtmlTemplate = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdShareTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoostReferrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShareLink = table.Column<string>(type: "text", nullable: false),
                    ClickCount = table.Column<int>(type: "integer", nullable: false),
                    RequiredClicks = table.Column<int>(type: "integer", nullable: false),
                    BoostEarned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BoostEarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoostReferrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoostReferrals_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoostReferrals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyerAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    MaxPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    Keywords = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuyerAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyerRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    MaxPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuyerRequests_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatReactions_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MiniGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameName = table.Column<string>(type: "text", nullable: false),
                    GameType = table.Column<string>(type: "text", nullable: false),
                    GameConfig = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CoinsPerPlay = table.Column<int>(type: "integer", nullable: false),
                    MinCoinsWon = table.Column<int>(type: "integer", nullable: false),
                    MaxCoinsWon = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiniGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointsExchanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeType = table.Column<string>(type: "text", nullable: false),
                    PointsUsed = table.Column<int>(type: "integer", nullable: false),
                    ValueReceived = table.Column<int>(type: "integer", nullable: false),
                    MobileNetwork = table.Column<string>(type: "text", nullable: true),
                    MobileNumber = table.Column<string>(type: "text", nullable: true),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsExchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointsExchanges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecentlyViewed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdId = table.Column<int>(type: "integer", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentlyViewed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecentlyViewed_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecentlyViewed_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SellerRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RaterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdId = table.Column<int>(type: "integer", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Review = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerRatings_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SellerRatings_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SellerRatings_Users_RaterId",
                        column: x => x.RaterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellerRatings_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SellerTips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdId = table.Column<int>(type: "integer", nullable: false),
                    TipType = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerTips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerTips_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SwapRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterAdId = table.Column<int>(type: "integer", nullable: false),
                    TargetAdId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwapRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwapRequests_Ads_RequesterAdId",
                        column: x => x.RequesterAdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SwapRequests_Ads_TargetAdId",
                        column: x => x.TargetAdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SwapRequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuyerRequestResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuyerRequestId = table.Column<int>(type: "integer", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerRequestResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuyerRequestResponses_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BuyerRequestResponses_BuyerRequests_BuyerRequestId",
                        column: x => x.BuyerRequestId,
                        principalTable: "BuyerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuyerRequestResponses_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePlayHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    CoinsSpent = table.Column<int>(type: "integer", nullable: false),
                    CoinsWon = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlayHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePlayHistories_MiniGames_GameId",
                        column: x => x.GameId,
                        principalTable: "MiniGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GamePlayHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdAnalytics_AdId",
                table: "AdAnalytics",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_AdHistories_AdId",
                table: "AdHistories",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_AdQualityScores_AdId",
                table: "AdQualityScores",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_AdRelistReminders_AdId",
                table: "AdRelistReminders",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_AdRelistReminders_UserId",
                table: "AdRelistReminders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BoostReferrals_AdId",
                table: "BoostReferrals",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_BoostReferrals_UserId",
                table: "BoostReferrals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerAlerts_UserId",
                table: "BuyerAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerRequestResponses_AdId",
                table: "BuyerRequestResponses",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerRequestResponses_BuyerRequestId",
                table: "BuyerRequestResponses",
                column: "BuyerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerRequestResponses_SellerId",
                table: "BuyerRequestResponses",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerRequests_BuyerId",
                table: "BuyerRequests",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatReactions_MessageId",
                table: "ChatReactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatReactions_UserId",
                table: "ChatReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayHistories_GameId",
                table: "GamePlayHistories",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayHistories_UserId",
                table: "GamePlayHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PointsExchanges_UserId",
                table: "PointsExchanges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewed_AdId",
                table: "RecentlyViewed",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyViewed_UserId",
                table: "RecentlyViewed",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerRatings_AdId",
                table: "SellerRatings",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerRatings_ConversationId",
                table: "SellerRatings",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerRatings_RaterId",
                table: "SellerRatings",
                column: "RaterId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerRatings_SellerId",
                table: "SellerRatings",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerTips_AdId",
                table: "SellerTips",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_RequesterAdId",
                table: "SwapRequests",
                column: "RequesterAdId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_RequesterId",
                table: "SwapRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_TargetAdId",
                table: "SwapRequests",
                column: "TargetAdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdAnalytics");

            migrationBuilder.DropTable(
                name: "AdHistories");

            migrationBuilder.DropTable(
                name: "AdQualityScores");

            migrationBuilder.DropTable(
                name: "AdRelistReminders");

            migrationBuilder.DropTable(
                name: "AdShareTemplates");

            migrationBuilder.DropTable(
                name: "BoostReferrals");

            migrationBuilder.DropTable(
                name: "BuyerAlerts");

            migrationBuilder.DropTable(
                name: "BuyerRequestResponses");

            migrationBuilder.DropTable(
                name: "ChatReactions");

            migrationBuilder.DropTable(
                name: "GamePlayHistories");

            migrationBuilder.DropTable(
                name: "PointsExchanges");

            migrationBuilder.DropTable(
                name: "RecentlyViewed");

            migrationBuilder.DropTable(
                name: "SellerRatings");

            migrationBuilder.DropTable(
                name: "SellerTips");

            migrationBuilder.DropTable(
                name: "SwapRequests");

            migrationBuilder.DropTable(
                name: "BuyerRequests");

            migrationBuilder.DropTable(
                name: "MiniGames");

            migrationBuilder.DropColumn(
                name: "LiveVerificationScheduledAt",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "LiveVerificationSessionId",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "LiveVerificationVideoUrl",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "VerificationTier",
                table: "KycInfos");

            migrationBuilder.DropColumn(
                name: "AutoHiddenAt",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "BoostedUntil",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ClickCount",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "FraudReportCount",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "IsBoosted",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "IsFraud",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "IsSpam",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "LastRepostedAt",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "SaveCount",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ShareCount",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "SpamReportCount",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "VoiceDescriptionUrl",
                table: "Ads");
        }
    }
}
