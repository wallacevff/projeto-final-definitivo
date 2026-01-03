using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContentVideoAnnotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentVideoAnnotations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentAttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimeMarkerSeconds = table.Column<double>(type: "float", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentVideoAnnotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentVideoAnnotations_ContentAttachments_ContentAttachmentId",
                        column: x => x.ContentAttachmentId,
                        principalTable: "ContentAttachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentVideoAnnotations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentVideoAnnotations_ContentAttachmentId",
                table: "ContentVideoAnnotations",
                column: "ContentAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentVideoAnnotations_CreatedById",
                table: "ContentVideoAnnotations",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentVideoAnnotations");
        }
    }
}
