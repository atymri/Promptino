namespace Promptino.Core.DTOs;

public static class PaginationDefaults
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;
}

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
};

// Opaque keyset cursor: Base64("createdAtTicks:id"). Stable under concurrent
// inserts, unlike offset paging which skips/duplicates rows mid-scroll.
public record CursorResult<T>(
    IReadOnlyList<T> Items,
    string? NextCursor)
{
    public static CursorResult<T> Empty() => new(Array.Empty<T>(), null);
};
