using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public class CommentLike : BaseEntity<Guid>
{
    [ForeignKey(nameof(User))]
    public Guid UserID { get; set; }
    [ForeignKey(nameof(Comment))]
    public Guid CommentID { get; set; }

    public ApplicationUser User { get; set; }
    public Comment Comment { get; set; }
}
