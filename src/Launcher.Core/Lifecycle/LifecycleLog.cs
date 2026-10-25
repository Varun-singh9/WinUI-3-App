using Launcher.Core.Diagnostics;

namespace Launcher.Core.Lifecycle;

/// <summary>
/// Writes lifecycle diagnostics entries. Entries carry only state names, activation kind, durations and stable error
/// codes, never user input, paths or exception messages (ADR-0026).
/// </summary>
internal static class LifecycleLog
{
    private static readonly IReadOnlyDictionary<string, string> NoMetadata = new Dictionary<string, string>();

    public static void Write(
        IDiagnostics diagnostics,
        LogLevel level,
        string component,
        string eventId,
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? metadata = null,
        TimeSpan? duration = null)
    {
        diagnostics.Write(new LogEntry(
            DateTimeOffset.UtcNow,
            level,
            component,
            eventId,
            null,
            duration,
            errorCode,
            metadata ?? NoMetadata));
    }

    public static Dictionary<string, string> ExceptionMetadata(Exception exception)
    {
        return new Dictionary<string, string> { ["exceptionType"] = exception.GetType().Name };
    }
}
