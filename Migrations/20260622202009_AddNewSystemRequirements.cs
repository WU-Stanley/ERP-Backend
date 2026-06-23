using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WUIAM.Migrations
{
    /// <inheritdoc />
    public partial class AddNewSystemRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "OfferLetters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "OfferLetters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeLevel",
                table: "OfferLetters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DependentLeaveTypeId",
                table: "LeavePolicies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateUrl",
                table: "EmployeeDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "EmployeeDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "EmployeeDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentificationUrl",
                table: "EmployeeDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_DependentLeaveTypeId",
                table: "LeavePolicies",
                column: "DependentLeaveTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeavePolicies_LeaveTypes_DependentLeaveTypeId",
                table: "LeavePolicies",
                column: "DependentLeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeavePolicies_LeaveTypes_DependentLeaveTypeId",
                table: "LeavePolicies");

            migrationBuilder.DropIndex(
                name: "IX_LeavePolicies_DependentLeaveTypeId",
                table: "LeavePolicies");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "GradeLevel",
                table: "OfferLetters");

            migrationBuilder.DropColumn(
                name: "DependentLeaveTypeId",
                table: "LeavePolicies");

            migrationBuilder.DropColumn(
                name: "CertificateUrl",
                table: "EmployeeDetails");

            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "EmployeeDetails");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "EmployeeDetails");

            migrationBuilder.DropColumn(
                name: "IdentificationUrl",
                table: "EmployeeDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "OfferLetters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
