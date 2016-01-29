using System;
using Microsoft.Data.Entity.Migrations;

namespace SOCVR.Chatbot.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    ProfileId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    LastTrackingPreferenceChange = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.ProfileId);
                });
            migrationBuilder.CreateTable(
                name: "PermissionRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    RequestedOn = table.Column<DateTimeOffset>(nullable: false),
                    RequestedPermissionGroup = table.Column<int>(nullable: false),
                    RequestingUserId = table.Column<int>(nullable: false),
                    ReviewingUserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionRequest_User_RequestingUserId",
                        column: x => x.RequestingUserId,
                        principalTable: "User",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionRequest_User_ReviewingUserId",
                        column: x => x.ReviewingUserId,
                        principalTable: "User",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "UserPermission",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    PermissionGroup = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermission_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
                name: "UserReviewedItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:Serial", true),
                    ActionTaken = table.Column<int>(nullable: false),
                    AuditPassed = table.Column<bool>(nullable: true),
                    PrimaryTag = table.Column<string>(nullable: false),
                    ReviewedOn = table.Column<DateTimeOffset>(nullable: false),
                    ReviewerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReviewedItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReviewedItem_User_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "User",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("PermissionRequest");
            migrationBuilder.DropTable("UserPermission");
            migrationBuilder.DropTable("UserReviewedItem");
            migrationBuilder.DropTable("User");
        }
    }
}
