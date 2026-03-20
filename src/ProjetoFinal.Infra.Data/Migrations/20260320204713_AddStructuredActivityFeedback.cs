using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinal.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredActivityFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationScore",
                table: "ActivitySubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommunicationScore",
                table: "ActivitySubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackTags",
                table: "ActivitySubmissions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MasteryScore",
                table: "ActivitySubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedAction",
                table: "ActivitySubmissions",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationScore",
                table: "ActivitySubmissions");

            migrationBuilder.DropColumn(
                name: "CommunicationScore",
                table: "ActivitySubmissions");

            migrationBuilder.DropColumn(
                name: "FeedbackTags",
                table: "ActivitySubmissions");

            migrationBuilder.DropColumn(
                name: "MasteryScore",
                table: "ActivitySubmissions");

            migrationBuilder.DropColumn(
                name: "RecommendedAction",
                table: "ActivitySubmissions");
        }
    }
}
