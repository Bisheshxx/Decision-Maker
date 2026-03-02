using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecisionMaker.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToDecisionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "DecisionItem",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionItem_CreatedById",
                table: "DecisionItem",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_DecisionItem_AspNetUsers_CreatedById",
                table: "DecisionItem",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DecisionItem_AspNetUsers_CreatedById",
                table: "DecisionItem");

            migrationBuilder.DropIndex(
                name: "IX_DecisionItem_CreatedById",
                table: "DecisionItem");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "DecisionItem");
        }
    }
}
