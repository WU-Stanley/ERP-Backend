using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WUIAM.Migrations
{
    /// <inheritdoc />
    public partial class AddMicrosoftAccountProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IctOnboardingStatus",
                table: "JobApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "NotStarted");

            migrationBuilder.AddColumn<DateTime>(
                name: "MicrosoftAccountProvisionedAt",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MicrosoftAccountProvisionedBy",
                table: "JobApplications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftProvisioningError",
                table: "JobApplications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftUserId",
                table: "JobApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftUserPrincipalName",
                table: "JobApplications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_MicrosoftUserPrincipalName",
                table: "JobApplications",
                column: "MicrosoftUserPrincipalName",
                unique: true,
                filter: "[MicrosoftUserPrincipalName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_MicrosoftUserPrincipalName",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "IctOnboardingStatus",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MicrosoftAccountProvisionedAt",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MicrosoftAccountProvisionedBy",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MicrosoftProvisioningError",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MicrosoftUserId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "MicrosoftUserPrincipalName",
                table: "JobApplications");
        }
    }
}
