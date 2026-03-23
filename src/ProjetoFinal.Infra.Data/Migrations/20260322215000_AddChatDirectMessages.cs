using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChatDirectMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecipientId",
                table: "ChatMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_RecipientId",
                table: "ChatMessages",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_RecipientId",
                table: "ChatMessages",
                column: "RecipientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_RecipientId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_RecipientId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "ChatMessages");
        }
    }
}
