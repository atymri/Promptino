using System.Globalization;
using System.Text;

namespace Promptino.Core.Services.PromptServices;

public static class FeedCursor
{
    public static string Encode(DateTimeOffset createdAt, Guid id)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{createdAt.UtcTicks.ToString(CultureInfo.InvariantCulture)}:{id}"));

    public static bool TryDecode(string? cursor, out DateTime createdAt, out Guid id)
    {
        createdAt = default;
        id = default;
        if (string.IsNullOrWhiteSpace(cursor)) return false;

        string raw;
        try { raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor)); }
        catch (FormatException) { return false; }

        var sep = raw.IndexOf(':');
        if (sep <= 0) return false;

        var ticksPart = raw[..sep];
        var idPart = raw[(sep + 1)..];

        if (!long.TryParse(ticksPart, CultureInfo.InvariantCulture, out var ticks)) return false;
        if (!Guid.TryParse(idPart, out id)) return false;

        try { createdAt = new DateTime(ticks, DateTimeKind.Utc); }
        catch (ArgumentOutOfRangeException) { return false; }

        return true;
    }
}
