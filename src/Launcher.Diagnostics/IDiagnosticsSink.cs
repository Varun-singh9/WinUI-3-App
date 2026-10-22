using Launcher.Core.Diagnostics;

namespace Launcher.Diagnostics;

/// <summary>
/// A destination for structured diagnostic entries, for example a log file.
/// </summary>
public interface IDiagnosticsSink
{
    /// <summary>Writes one entry. The caller guards against exceptions, so a failing sink never reaches product code.</summary>
    /// <param name="entry">The entry to write.</param>
    void Write(LogEntry entry);
}
