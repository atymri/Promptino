using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public class Comment : BaseEntity<Guid>
{
    [ForeignKey(nameof(User))]
    public Guid UserID { get; set; }
    [ForeignKey(nameof(Prompt))]
    public Guid PromptID { get; set; }
    [ForeignKey(nameof(ParentComment))]
    public Guid? ParentCommentID { get; set; }
    public string Content { get; set; }

    public ApplicationUser User { get; set; }
    public Prompt Prompt { get; set; }
    public virtual Comment? ParentComment { get; set; }
    public virtual List<Comment> Replies { get; set; } = new();
    public virtual List<CommentLike> Likes { get; set; } = new();
}
