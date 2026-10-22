using Launcher.Core.Diagnostics;

namespace Launcher.Diagnostics;

/// <summary>
/// Defence in depth for the rule that diagnostics never contain secrets or raw search text. It replaces the values
/// of metadata keys that look sensitive. This is a safety net, not a substitute for callers keeping such data out of entries.
/// </summary>
internal static class MetadataRedactor
{
    internal const string Redacted = "[redacted]";

    private static readonly HashSet<string> ExactKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "query", "searchtext", "rawinput", "input", "text",
    };

    private static readonly string[] KeyFragments =
    [
        "password", "passwd", "secret", "token", "apikey", "api_key", "credential", "authorization",
    ];

    /// <summary>Returns the entry with sensitive metadata values replaced; the entry is returned unchanged when nothing needs redaction.</summary>
    public static LogEntry Redact(LogEntry entry)
    {
        if (entry.Metadata is null || entry.Metadata.Count == 0 || !entry.Metadata.Keys.Any(IsSensitive))
        {
            return entry;
        }

        var safe = entry.Metadata.ToDictionary(
            pair => pair.Key,
            pair => IsSensitive(pair.Key) ? Redacted : pair.Value,
            StringComparer.Ordinal);

        return entry with { Metadata = safe };
    }

    private static bool IsSensitive(string key)
    {
        return ExactKeys.Contains(key)
            || KeyFragments.Any(fragment => key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
