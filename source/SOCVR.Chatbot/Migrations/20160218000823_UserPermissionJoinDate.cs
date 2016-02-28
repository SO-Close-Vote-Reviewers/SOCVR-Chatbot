using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace SOCVR.Chatbot.Migrations
{
    public partial class UserPermissionJoinDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_PermissionRequest_User_RequestingUserId", table: "PermissionRequest");
            migrationBuilder.DropForeignKey(name: "FK_UserPermission_User_UserId", table: "UserPermission");
            migrationBuilder.DropForeignKey(name: "FK_UserReviewedItem_User_ReviewerId", table: "UserReviewedItem");
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "JoinedOn",
                table: "UserPermission",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRequest_User_RequestingUserId",
                table: "PermissionRequest",
                column: "RequestingUserId",
                principalTable: "User",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_User_UserId",
                table: "UserPermission",
                column: "UserId",
                principalTable: "User",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_UserReviewedItem_User_ReviewerId",
                table: "UserReviewedItem",
                column: "ReviewerId",
                principalTable: "User",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_PermissionRequest_User_RequestingUserId", table: "PermissionRequest");
            migrationBuilder.DropForeignKey(name: "FK_UserPermission_User_UserId", table: "UserPermission");
            migrationBuilder.DropForeignKey(name: "FK_UserReviewedItem_User_ReviewerId", table: "UserReviewedItem");
            migrationBuilder.DropColumn(name: "JoinedOn", table: "UserPermission");
            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRequest_User_RequestingUserId",
                table: "PermissionRequest",
                column: "RequestingUserId",
                principalTable: "User",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_User_UserId",
                table: "UserPermission",
                column: "UserId",
                principalTable: "User",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_UserReviewedItem_User_ReviewerId",
                table: "UserReviewedItem",
                column: "ReviewerId",
                principalTable: "User",
                principalColumn: "ProfileId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
