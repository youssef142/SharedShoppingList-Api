using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyShoppingList.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAnalyticsEditRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "ShoppingItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemEditRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestType = table.Column<int>(type: "INTEGER", nullable: false),
                    PreviousCategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProposedCategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PreviousStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    ProposedStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    PreviousAddedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProposedAddedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PreviousStatusChangedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProposedStatusChangedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RequestStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemEditRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemEditRequests_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemEditRequests_ShoppingItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ShoppingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemEditRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemEditRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItems_CategoryId",
                table: "ShoppingItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_GroupId_Name",
                table: "Categories",
                columns: new[] { "GroupId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemEditRequests_GroupId",
                table: "ItemEditRequests",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEditRequests_ItemId",
                table: "ItemEditRequests",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEditRequests_RequestedByUserId",
                table: "ItemEditRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEditRequests_ReviewedByUserId",
                table: "ItemEditRequests",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingItems_Categories_CategoryId",
                table: "ShoppingItems",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingItems_Categories_CategoryId",
                table: "ShoppingItems");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "ItemEditRequests");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingItems_CategoryId",
                table: "ShoppingItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ShoppingItems");
        }
    }
}
