using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cadmus.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectorEventDeduplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceEventId",
                table: "PrintJobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_SourceEventId",
                table: "PrintJobs",
                column: "SourceEventId",
                unique: true,
                filter: "[SourceEventId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrintJobs_SourceEventId",
                table: "PrintJobs");

            migrationBuilder.DropColumn(
                name: "SourceEventId",
                table: "PrintJobs");
        }
    }
}
