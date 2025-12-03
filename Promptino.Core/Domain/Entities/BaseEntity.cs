using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Promptino.Core.Domain.Entities;

public class BaseEntity<T> where T : notnull
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public T ID { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    public void Touch()
    {
        LastUpdatedAt = DateTime.UtcNow;
    }
}
