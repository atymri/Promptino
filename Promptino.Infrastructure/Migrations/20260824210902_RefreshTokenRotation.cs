using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promptino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenRotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousRefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousRefreshTokenExpiration",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            // Full-text search over prompts. EF cannot scaffold FTS DDL; these are
            // idempotent and tolerate environments where the Full-Text Engine feature
            // is not installed (e.g. SQL Express basic), in which case search
            // falls back to LIKE at runtime.
            migrationBuilder.Sql(@"
IF (SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled')) = 1
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'PromptinoCatalog')
        CREATE FULLTEXT CATALOG PromptinoCatalog AS DEFAULT;

    IF NOT EXISTS (
        SELECT 1 FROM sys.fulltext_indexes fi
        JOIN sys.objects o ON fi.object_id = o.object_id
        WHERE o.name = N'Prompts')
    BEGIN
        CREATE UNIQUE INDEX UX_Prompts_FTS ON Prompts(ID);
        CREATE FULLTEXT INDEX ON Prompts(Title, Description, Content)
            KEY INDEX UX_Prompts_FTS
            ON PromptinoCatalog
            WITH STOPLIST = SYSTEM, CHANGE_TRACKING AUTO;
    END
END", suppressTransaction: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.fulltext_indexes fi
    JOIN sys.objects o ON fi.object_id = o.object_id
    WHERE o.name = N'Prompts')
    DROP FULLTEXT INDEX ON Prompts;

IF OBJECT_ID(N'UX_Prompts_FTS', N'INDEX') IS NOT NULL
    DROP INDEX UX_Prompts_FTS ON Prompts;");

            migrationBuilder.DropColumn(
                name: "PreviousRefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PreviousRefreshTokenExpiration",
                table: "AspNetUsers");
        }
    }
}
