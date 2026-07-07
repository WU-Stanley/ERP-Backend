using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WUIAM.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityAssetToHelpdeskTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FacilityAssetId",
                table: "HelpdeskTickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTickets_FacilityAssetId",
                table: "HelpdeskTickets",
                column: "FacilityAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpdeskTickets_FacilityAssets_FacilityAssetId",
                table: "HelpdeskTickets",
                column: "FacilityAssetId",
                principalTable: "FacilityAssets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpdeskTickets_FacilityAssets_FacilityAssetId",
                table: "HelpdeskTickets");

            migrationBuilder.DropIndex(
                name: "IX_HelpdeskTickets_FacilityAssetId",
                table: "HelpdeskTickets");

            migrationBuilder.DropColumn(
                name: "FacilityAssetId",
                table: "HelpdeskTickets");
        }
    }
}
