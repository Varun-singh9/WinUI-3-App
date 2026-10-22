using Launcher.Core.Diagnostics;

namespace Launcher.Diagnostics;

/// <summary>
/// Keeps entries in memory. Intended for tests and for a future in-app diagnostics view.
/// </summary>
public sealed class MemoryDiagnosticsSink : IDiagnosticsSink
{
    private readonly object _gate = new();
    private readonly List<LogEntry> _entries = [];

    /// <summary>Gets a snapshot of the entries written so far.</summary>
    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return _entries.ToList();
            }
        }
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        lock (_gate)
        {
            _entries.Add(entry);
        }
    }
}
