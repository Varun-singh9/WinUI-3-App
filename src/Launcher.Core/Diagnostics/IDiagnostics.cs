namespace Launcher.Core.Diagnostics;

/// <summary>
/// Receives structured diagnostics. Diagnostics are cross-cutting infrastructure and must never become a hidden
/// product dependency, so writing an entry must not throw into the caller.
/// </summary>
public interface IDiagnostics
{
    /// <summary>Writes one structured entry.</summary>
    /// <param name="entry">The entry to write.</param>
    void Write(LogEntry entry);
}
