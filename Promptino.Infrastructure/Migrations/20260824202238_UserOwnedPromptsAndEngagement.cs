using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promptino.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserOwnedPromptsAndEngagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: some environments (e.g. after the transient _Auto migration)
            // no longer have this table when this migration is applied
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.FavoritePrompts', N'U') IS NOT NULL
    DROP TABLE [FavoritePrompts];");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Prompts', N'UserID') IS NULL
BEGIN
    ALTER TABLE [Prompts] ADD [UserID] uniqueidentifier NOT NULL
        CONSTRAINT DF_Prompts_UserID DEFAULT '00000000-0000-0000-0000-000000000000';
END");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefreshTokenExpiration",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");


            // All schema objects below are created idempotently: some environments carry a
            // partial schema from the transient automatic migration, so every statement
            // checks for existence before applying. Fresh databases run everything.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE [Categories] (
        [ID] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([ID])
    );
END

IF OBJECT_ID(N'dbo.Comments', N'U') IS NULL
BEGIN
    CREATE TABLE [Comments] (
        [ID] uniqueidentifier NOT NULL,
        [UserID] uniqueidentifier NOT NULL,
        [PromptID] uniqueidentifier NOT NULL,
        [ParentCommentID] uniqueidentifier NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_Comments_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Comments_Comments_ParentCommentID] FOREIGN KEY ([ParentCommentID]) REFERENCES [Comments] ([ID]),
        CONSTRAINT [FK_Comments_Prompts_PromptID] FOREIGN KEY ([PromptID]) REFERENCES [Prompts] ([ID]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'dbo.PromptReactions', N'U') IS NULL
BEGIN
    CREATE TABLE [PromptReactions] (
        [ID] uniqueidentifier NOT NULL,
        [UserID] uniqueidentifier NOT NULL,
        [PromptID] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PromptReactions] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_PromptReactions_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_PromptReactions_Prompts_PromptID] FOREIGN KEY ([PromptID]) REFERENCES [Prompts] ([ID]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'dbo.SavedPrompts', N'U') IS NULL
BEGIN
    CREATE TABLE [SavedPrompts] (
        [ID] uniqueidentifier NOT NULL,
        [UserID] uniqueidentifier NOT NULL,
        [PromptID] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SavedPrompts] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_SavedPrompts_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_SavedPrompts_Prompts_PromptID] FOREIGN KEY ([PromptID]) REFERENCES [Prompts] ([ID]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'dbo.PromptCategories', N'U') IS NULL
BEGIN
    CREATE TABLE [PromptCategories] (
        [ID] uniqueidentifier NOT NULL,
        [PromptId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PromptCategories] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_PromptCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_PromptCategories_Prompts_PromptId] FOREIGN KEY ([PromptId]) REFERENCES [Prompts] ([ID]) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'dbo.CommentLikes', N'U') IS NULL
BEGIN
    CREATE TABLE [CommentLikes] (
        [ID] uniqueidentifier NOT NULL,
        [UserID] uniqueidentifier NOT NULL,
        [CommentID] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastUpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CommentLikes] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_CommentLikes_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommentLikes_Comments_CommentID] FOREIGN KEY ([CommentID]) REFERENCES [Comments] ([ID]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommentLikes_CommentID' AND object_id = OBJECT_ID(N'dbo.CommentLikes'))
    CREATE INDEX [IX_CommentLikes_CommentID] ON [CommentLikes] ([CommentID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommentLikes_UserID_CommentID' AND object_id = OBJECT_ID(N'dbo.CommentLikes'))
    CREATE UNIQUE INDEX [IX_CommentLikes_UserID_CommentID] ON [CommentLikes] ([UserID], [CommentID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Comments_ParentCommentID' AND object_id = OBJECT_ID(N'dbo.Comments'))
    CREATE INDEX [IX_Comments_ParentCommentID] ON [Comments] ([ParentCommentID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Comments_PromptID' AND object_id = OBJECT_ID(N'dbo.Comments'))
    CREATE INDEX [IX_Comments_PromptID] ON [Comments] ([PromptID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Comments_UserID' AND object_id = OBJECT_ID(N'dbo.Comments'))
    CREATE INDEX [IX_Comments_UserID] ON [Comments] ([UserID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PromptCategories_CategoryId' AND object_id = OBJECT_ID(N'dbo.PromptCategories'))
    CREATE INDEX [IX_PromptCategories_CategoryId] ON [PromptCategories] ([CategoryId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PromptCategories_PromptId' AND object_id = OBJECT_ID(N'dbo.PromptCategories'))
    CREATE INDEX [IX_PromptCategories_PromptId] ON [PromptCategories] ([PromptId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PromptReactions_PromptID' AND object_id = OBJECT_ID(N'dbo.PromptReactions'))
    CREATE INDEX [IX_PromptReactions_PromptID] ON [PromptReactions] ([PromptID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PromptReactions_UserID_PromptID' AND object_id = OBJECT_ID(N'dbo.PromptReactions'))
    CREATE UNIQUE INDEX [IX_PromptReactions_UserID_PromptID] ON [PromptReactions] ([UserID], [PromptID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SavedPrompts_PromptID' AND object_id = OBJECT_ID(N'dbo.SavedPrompts'))
    CREATE INDEX [IX_SavedPrompts_PromptID] ON [SavedPrompts] ([PromptID]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SavedPrompts_UserID' AND object_id = OBJECT_ID(N'dbo.SavedPrompts'))
    CREATE INDEX [IX_SavedPrompts_UserID] ON [SavedPrompts] ([UserID]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prompts_AspNetUsers_UserID",
                table: "Prompts");

            migrationBuilder.DropTable(
                name: "CommentLikes");

            migrationBuilder.DropTable(
                name: "PromptCategories");

            migrationBuilder.DropTable(
                name: "PromptReactions");

            migrationBuilder.DropTable(
                name: "SavedPrompts");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Prompts_UserID",
                table: "Prompts");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Prompts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefreshTokenExpiration",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "FavoritePrompts",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritePrompts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FavoritePrompts_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritePrompts_Prompts_PromptID",
                        column: x => x.PromptID,
                        principalTable: "Prompts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePrompts_PromptID",
                table: "FavoritePrompts",
                column: "PromptID");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritePrompts_UserID",
                table: "FavoritePrompts",
                column: "UserID");
        }
    }
}
