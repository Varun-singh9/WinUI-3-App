using Launcher.Core.Diagnostics;

namespace Launcher.Diagnostics;

/// <summary>
/// The structured diagnostics entry point. It filters by level, redacts sensitive metadata and fans entries out to
/// sinks. Writing never throws into the caller, because diagnostics must not become a hidden product dependency.
/// </summary>
public sealed class DiagnosticsService : IDiagnostics
{
    private readonly IReadOnlyList<IDiagnosticsSink> _sinks;
    private readonly LogLevel _minimumLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsService"/> class.
    /// </summary>
    /// <param name="sinks">The destinations that receive entries.</param>
    /// <param name="minimumLevel">Entries below this level are dropped.</param>
    public DiagnosticsService(IEnumerable<IDiagnosticsSink> sinks, LogLevel minimumLevel = LogLevel.Information)
    {
        _sinks = sinks.ToList();
        _minimumLevel = minimumLevel;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        try
        {
            if (entry is null || entry.Level < _minimumLevel)
            {
                return;
            }

            var safe = MetadataRedactor.Redact(entry);
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Write(safe);
                }
                catch (Exception)
                {
                    // A failing sink must not stop the other sinks or reach product code.
                }
            }
        }
        catch (Exception)
        {
            // Diagnostics never throw into the caller.
        }
    }
}
