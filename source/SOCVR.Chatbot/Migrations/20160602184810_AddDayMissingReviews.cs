using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace SOCVR.Chatbot.Migrations
{
    public partial class AddDayMissingReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DayMissingReviews",
                columns: table => new
                {
                    ProfileId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    MissingReviewsCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayMissingReviews", x => new { x.ProfileId, x.Date });
                    table.ForeignKey(
                        name: "FK_DayMissingReviews_User_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "User",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("DayMissingReviews");
        }
    }
}
