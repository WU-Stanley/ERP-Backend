using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WUIAM.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "InterviewInterviewers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackStatus",
                table: "InterviewInterviewers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "InterviewInterviewers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recommendation",
                table: "InterviewInterviewers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "InterviewInterviewers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "InterviewInterviewers");

            migrationBuilder.DropColumn(
                name: "FeedbackStatus",
                table: "InterviewInterviewers");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "InterviewInterviewers");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "InterviewInterviewers");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "InterviewInterviewers");
        }
    }
}
