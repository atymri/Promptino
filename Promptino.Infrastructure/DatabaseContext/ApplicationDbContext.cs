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
    public virtual DbSet<FavoritePrompts> FavoritePrompts { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<PromptCategories> PromptCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

