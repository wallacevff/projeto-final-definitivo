using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiAttentionPointsJson",
                table: "CourseContents",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiKeyPointsJson",
                table: "CourseContents",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSummary",
                table: "CourseContents",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiSummaryGeneratedAt",
                table: "CourseContents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AiSummaryGeneratedById",
                table: "CourseContents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiSummaryModel",
                table: "CourseContents",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAttentionPointsJson",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "AiKeyPointsJson",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "AiSummary",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "AiSummaryGeneratedAt",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "AiSummaryGeneratedById",
                table: "CourseContents");

            migrationBuilder.DropColumn(
                name: "AiSummaryModel",
                table: "CourseContents");
        }
    }
}
