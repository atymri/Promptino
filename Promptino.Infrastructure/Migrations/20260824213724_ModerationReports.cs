using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promptino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModerationReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Prompts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PromptReports",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporterID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResolvedByUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptReports", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PromptReports_AspNetUsers_ReporterID",
                        column: x => x.ReporterID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromptReports_Prompts_PromptID",
                        column: x => x.PromptID,
                        principalTable: "Prompts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptReports_PromptID",
                table: "PromptReports",
                column: "PromptID");

            migrationBuilder.CreateIndex(
                name: "IX_PromptReports_ReporterID_PromptID",
                table: "PromptReports",
                columns: new[] { "ReporterID", "PromptID" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptReports");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Prompts");
        }
    }
}
