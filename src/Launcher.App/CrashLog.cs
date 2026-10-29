using Launcher.Core.Diagnostics;

namespace Launcher.App;

/// <summary>
/// Writes the entries only the app can write: a startup or shutdown failure and the unhandled-exception sources
/// (ADR-0028). It goes through the same <see cref="IDiagnostics"/> as every component and adds no sink or format.
/// Only the exception type and HResult are recorded, never the message or the stack, because those can contain
/// user paths.
/// </summary>
internal static class CrashLog
{
    private const string Component = "App";

    /// <summary>Records an exception from the named source.</summary>
    /// <param name="diagnostics">The shared diagnostics.</param>
    /// <param name="source">Where it came from, for example <c>startup</c> or <c>application</c>.</param>
    /// <param name="exception">The exception, or <see langword="null"/> when the source gave none.</param>
    /// <param name="fatal">Whether the process is about to end because of it.</param>
    public static void Write(IDiagnostics diagnostics, string source, Exception? exception, bool fatal)
    {
        var eventId = fatal ? "app.unhandled_exception" : "app.exception";
        var metadata = new Dictionary<string, string>
        {
            ["source"] = source,
            ["exceptionType"] = exception is null ? "unknown" : TypeNames(exception),
            ["hresult"] = exception is null ? "none" : $"0x{exception.HResult:X8}",
        };

        diagnostics.Write(new LogEntry(
            DateTimeOffset.UtcNow,
            fatal ? LogLevel.Critical : LogLevel.Error,
            Component,
            eventId,
            null,
            null,
            eventId,
            metadata));
    }

    private static string TypeNames(Exception exception)
    {
        if (exception is AggregateException aggregate)
        {
            var inner = aggregate.Flatten().InnerExceptions.Select(e => e.GetType().Name).Distinct().Take(5).ToArray();
            return inner.Length == 0 ? nameof(AggregateException) : string.Join(",", inner);
        }

        return exception.GetType().Name;
    }
}
