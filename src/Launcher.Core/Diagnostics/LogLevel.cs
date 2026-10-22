namespace Launcher.Core.Diagnostics;

/// <summary>
/// Severity of a diagnostic entry.
/// </summary>
public enum LogLevel
{
    /// <summary>Very detailed developer tracing.</summary>
    Trace,

    /// <summary>Developer debugging information.</summary>
    Debug,

    /// <summary>Normal operational events.</summary>
    Information,

    /// <summary>Something unexpected that the launcher recovered from.</summary>
    Warning,

    /// <summary>A failure of one operation.</summary>
    Error,

    /// <summary>A failure that threatens the launcher as a whole.</summary>
    Critical,
}
