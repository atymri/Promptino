using System.ComponentModel.DataAnnotations;

namespace Promptino.Core.Domain.Entities;

public class BaseEntity<T> where T : notnull
{
    [Key]
    public T ID { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    void Touch()
    {
        LastUpdatedAt = DateTime.UtcNow;
    }
}
