using Microsoft.Data.Entity.Migrations;

namespace SOCVR.Chatbot.Migrations
{
    public partial class UpdatePK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_PermissionRequest_User_RequestingUserId", table: "PermissionRequest");
            migrationBuilder.DropForeignKey(name: "FK_UserPermission_User_UserId", table: "UserPermission");
            migrationBuilder.DropForeignKey(name: "FK_UserReviewedItem_User_ReviewerId", table: "UserReviewedItem");
            migrationBuilder.DropPrimaryKey(name: "PK_UserReviewedItem", table: "UserReviewedItem");
            migrationBuilder.AlterColumn<int>(
                name: "ReviewId",
                table: "UserReviewedItem",
                nullable: false);
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserReviewedItem",
                table: "UserReviewedItem",
                columns: new[] { "ReviewId", "ReviewerId" });
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
            migrationBuilder.DropPrimaryKey(name: "PK_UserReviewedItem", table: "UserReviewedItem");
            migrationBuilder.AlterColumn<int>(
                name: "ReviewId",
                table: "UserReviewedItem",
                nullable: false)
                .Annotation("Npgsql:Serial", true);
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserReviewedItem",
                table: "UserReviewedItem",
                column: "ReviewId");
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
