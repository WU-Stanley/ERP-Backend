using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WUIAM.Migrations
{
    /// <inheritdoc />
    public partial class AddImpersonatorIdToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImpersonatorId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ImpersonatorId",
                table: "AuditLogs",
                column: "ImpersonatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_ImpersonatorId",
                table: "AuditLogs",
                column: "ImpersonatorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_ImpersonatorId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_ImpersonatorId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ImpersonatorId",
                table: "AuditLogs");
        }
    }
}
