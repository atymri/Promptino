using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Promptino.Core.Domain.Entities;

namespace Promptino.Infrastructure.DatabaseContext;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Prompt> Prompts { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<PromptImage> PromptImages { get; set; }
    public virtual DbSet<SavedPrompt> SavedPrompts { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<PromptCategories> PromptCategories { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<PromptReaction> PromptReactions { get; set; }
    public virtual DbSet<CommentLike> CommentLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PromptReaction>()
            .HasIndex(r => new { r.UserID, r.PromptID })
            .IsUnique();

        // SQL Server rejects multiple cascade paths (e.g. Users→Prompts→Comments alongside
        // Users→Comments); every engagement table keeps Cascade only from Prompt, while all
        // User-side FKs are NoAction so deleting a user never auto-fans out through prompts.
        // The WithMany() sides target the ApplicationUser collections so no shadow FKs appear.
        modelBuilder.Entity<Prompt>()
            .HasOne(p => p.User)
            .WithMany(u => u.Prompts)
            .HasForeignKey(p => p.UserID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PromptReaction>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reactions)
            .HasForeignKey(r => r.UserID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SavedPrompt>()
            .HasOne(s => s.User)
            .WithMany(u => u.SavedPrompts)
            .HasForeignKey(s => s.UserID)
            .OnDelete(DeleteBehavior.NoAction);

        // SQL Server does not execute recursive same-table cascades; replies are removed app-side
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CommentLike>()
            .HasIndex(l => new { l.UserID, l.CommentID })
            .IsUnique();
    }
}

