using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WUIAM.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelForReseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_EmployeeDetails_EmployeeId",
                table: "EmployeeProfileUpdateRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_Users_RequestedByUserId",
                table: "EmployeeProfileUpdateRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_Users_ReviewedByUserId",
                table: "EmployeeProfileUpdateRequests");

            migrationBuilder.DropIndex(
                name: "IX_SalaryStructures_Code",
                table: "SalaryStructures");

            migrationBuilder.DropIndex(
                name: "IX_RegistryIntegrationRecords_Status",
                table: "RegistryIntegrationRecords");

            migrationBuilder.DropIndex(
                name: "IX_RegistryIntegrationRecords_SystemName",
                table: "RegistryIntegrationRecords");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementRequests_DepartmentId",
                table: "ProcurementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementRequests_RequestedByUserId",
                table: "ProcurementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementRequests_RequestNumber",
                table: "ProcurementRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProcurementRequests_Status",
                table: "ProcurementRequests");

            migrationBuilder.DropIndex(
                name: "IX_PayrollRuns_PeriodName",
                table: "PayrollRuns");

            migrationBuilder.DropIndex(
                name: "IX_PayrollRuns_ProcessedByUserId",
                table: "PayrollRuns");

            migrationBuilder.DropIndex(
                name: "IX_PayrollRuns_Status",
                table: "PayrollRuns");

            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_UserId_LeaveTypeId_ValidFrom",
                table: "LeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_Category",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_Sku",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_Status",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_HelpdeskTickets_AssigneeUserId",
                table: "HelpdeskTickets");

            migrationBuilder.DropIndex(
                name: "IX_HelpdeskTickets_RequesterUserId",
                table: "HelpdeskTickets");

            migrationBuilder.DropIndex(
                name: "IX_HelpdeskTickets_Status",
                table: "HelpdeskTickets");

            migrationBuilder.DropIndex(
                name: "IX_HelpdeskTickets_TicketNumber",
                table: "HelpdeskTickets");

            migrationBuilder.DropIndex(
                name: "IX_FacilityAssets_AssetTag",
                table: "FacilityAssets");

            migrationBuilder.DropIndex(
                name: "IX_FacilityAssets_Category",
                table: "FacilityAssets");

            migrationBuilder.DropIndex(
                name: "IX_FacilityAssets_CustodianEmployeeId",
                table: "FacilityAssets");

            migrationBuilder.DropIndex(
                name: "IX_FacilityAssets_Status",
                table: "FacilityAssets");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentDetails_EmploymentStatus",
                table: "EmploymentDetails");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentDetails_IsActive",
                table: "EmploymentDetails");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeProfileUpdateRequests_RequestedByUserId",
                table: "EmployeeProfileUpdateRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeProfileUpdateRequests_ReviewedByUserId",
                table: "EmployeeProfileUpdateRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDetails_Email",
                table: "EmployeeDetails");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDetails_PhoneNumber",
                table: "EmployeeDetails");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRecords_OwnerDepartmentId",
                table: "DocumentRecords");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRecords_OwnerUserId",
                table: "DocumentRecords");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRecords_Status",
                table: "DocumentRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Departments");

            migrationBuilder.RenameColumn(
                name: "ReviewedByUserId",
                table: "EmployeeProfileUpdateRequests",
                newName: "ReviewedByUserID");

            migrationBuilder.RenameColumn(
                name: "RequestedByUserId",
                table: "EmployeeProfileUpdateRequests",
                newName: "RequestedByUserID");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeProfileUpdateRequests",
                newName: "EmployeeID");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeProfileUpdateRequests_EmployeeId",
                table: "EmployeeProfileUpdateRequests",
                newName: "IX_EmployeeProfileUpdateRequests_EmployeeID");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SalaryStructures",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "GradeLevel",
                table: "SalaryStructures",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "SalaryStructures",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "SystemName",
                table: "RegistryIntegrationRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(140)",
                oldMaxLength: 140);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "RegistryIntegrationRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "IntegrationType",
                table: "RegistryIntegrationRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ProcurementRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ProcurementRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "RequestNumber",
                table: "ProcurementRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "ProcurementRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PayrollRuns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "PeriodName",
                table: "PayrollRuns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "HelpdeskTickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "HelpdeskTickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HelpdeskTickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "HelpdeskTickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "HelpdeskTickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FacilityAssets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FacilityAssets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "FacilityAssets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Condition",
                table: "FacilityAssets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "FacilityAssets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AssetTag",
                table: "FacilityAssets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "EmploymentStatus",
                table: "EmploymentDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "EmployeeProfileUpdateRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "EmployeeDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EmployeeDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DocumentRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DocumentRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Confidentiality",
                table: "DocumentRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "DocumentRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_EmployeeDetails_EmployeeID",
                table: "EmployeeProfileUpdateRequests",
                column: "EmployeeID",
                principalTable: "EmployeeDetails",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_EmployeeDetails_EmployeeID",
                table: "EmployeeProfileUpdateRequests");

            migrationBuilder.RenameColumn(
                name: "ReviewedByUserID",
                table: "EmployeeProfileUpdateRequests",
                newName: "ReviewedByUserId");

            migrationBuilder.RenameColumn(
                name: "RequestedByUserID",
                table: "EmployeeProfileUpdateRequests",
                newName: "RequestedByUserId");

            migrationBuilder.RenameColumn(
                name: "EmployeeID",
                table: "EmployeeProfileUpdateRequests",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeProfileUpdateRequests_EmployeeID",
                table: "EmployeeProfileUpdateRequests",
                newName: "IX_EmployeeProfileUpdateRequests_EmployeeId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SalaryStructures",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GradeLevel",
                table: "SalaryStructures",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "SalaryStructures",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SystemName",
                table: "RegistryIntegrationRecords",
                type: "nvarchar(140)",
                maxLength: 140,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "RegistryIntegrationRecords",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IntegrationType",
                table: "RegistryIntegrationRecords",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ProcurementRequests",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ProcurementRequests",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "RequestNumber",
                table: "ProcurementRequests",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "ProcurementRequests",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PayrollRuns",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PeriodName",
                table: "PayrollRuns",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InventoryItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "InventoryItems",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InventoryItems",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "InventoryItems",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "InventoryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "HelpdeskTickets",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "HelpdeskTickets",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HelpdeskTickets",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "HelpdeskTickets",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "HelpdeskTickets",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FacilityAssets",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FacilityAssets",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "FacilityAssets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Condition",
                table: "FacilityAssets",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "FacilityAssets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetTag",
                table: "FacilityAssets",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmploymentStatus",
                table: "EmploymentDetails",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "EmployeeProfileUpdateRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "EmployeeDetails",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "EmployeeDetails",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "DocumentRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DocumentRecords",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Confidentiality",
                table: "DocumentRecords",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "DocumentRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Departments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructures_Code",
                table: "SalaryStructures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistryIntegrationRecords_Status",
                table: "RegistryIntegrationRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RegistryIntegrationRecords_SystemName",
                table: "RegistryIntegrationRecords",
                column: "SystemName");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_DepartmentId",
                table: "ProcurementRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_RequestedByUserId",
                table: "ProcurementRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_RequestNumber",
                table: "ProcurementRequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRequests_Status",
                table: "ProcurementRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_PeriodName",
                table: "PayrollRuns",
                column: "PeriodName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_ProcessedByUserId",
                table: "PayrollRuns",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRuns_Status",
                table: "PayrollRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_UserId_LeaveTypeId_ValidFrom",
                table: "LeaveBalances",
                columns: new[] { "UserId", "LeaveTypeId", "ValidFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Category",
                table: "InventoryItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Sku",
                table: "InventoryItems",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Status",
                table: "InventoryItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTickets_AssigneeUserId",
                table: "HelpdeskTickets",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTickets_RequesterUserId",
                table: "HelpdeskTickets",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTickets_Status",
                table: "HelpdeskTickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTickets_TicketNumber",
                table: "HelpdeskTickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityAssets_AssetTag",
                table: "FacilityAssets",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityAssets_Category",
                table: "FacilityAssets",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityAssets_CustodianEmployeeId",
                table: "FacilityAssets",
                column: "CustodianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityAssets_Status",
                table: "FacilityAssets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentDetails_EmploymentStatus",
                table: "EmploymentDetails",
                column: "EmploymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentDetails_IsActive",
                table: "EmploymentDetails",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfileUpdateRequests_RequestedByUserId",
                table: "EmployeeProfileUpdateRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfileUpdateRequests_ReviewedByUserId",
                table: "EmployeeProfileUpdateRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_Email",
                table: "EmployeeDetails",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_PhoneNumber",
                table: "EmployeeDetails",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_OwnerDepartmentId",
                table: "DocumentRecords",
                column: "OwnerDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_OwnerUserId",
                table: "DocumentRecords",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_Status",
                table: "DocumentRecords",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_EmployeeDetails_EmployeeId",
                table: "EmployeeProfileUpdateRequests",
                column: "EmployeeId",
                principalTable: "EmployeeDetails",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_Users_RequestedByUserId",
                table: "EmployeeProfileUpdateRequests",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeProfileUpdateRequests_Users_ReviewedByUserId",
                table: "EmployeeProfileUpdateRequests",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
