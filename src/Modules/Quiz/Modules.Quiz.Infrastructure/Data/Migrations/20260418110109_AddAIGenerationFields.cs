using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Quiz.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIGenerationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Complexity",
                schema: "Question",
                table: "QuestionSets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                schema: "Question",
                table: "QuestionSets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpertiseFields",
                schema: "Question",
                table: "QuestionSets",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                schema: "Question",
                table: "QuestionSets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                schema: "Question",
                table: "QuestionSets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DifficultyScore",
                schema: "Question",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                schema: "Question",
                table: "Questions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionType",
                schema: "Question",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                schema: "Question",
                table: "Questions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionIdentifier",
                schema: "Question",
                table: "QuestionOptions",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complexity",
                schema: "Question",
                table: "QuestionSets");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                schema: "Question",
                table: "QuestionSets");

            migrationBuilder.DropColumn(
                name: "ExpertiseFields",
                schema: "Question",
                table: "QuestionSets");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                schema: "Question",
                table: "QuestionSets");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "Question",
                table: "QuestionSets");

            migrationBuilder.DropColumn(
                name: "DifficultyScore",
                schema: "Question",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Explanation",
                schema: "Question",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                schema: "Question",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "Question",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "OptionIdentifier",
                schema: "Question",
                table: "QuestionOptions");
        }
    }
}
