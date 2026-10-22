namespace Launcher.Core.Diagnostics;

/// <summary>
/// One structured diagnostic event (Implementation Specification section 21). Important paths use structured events,
/// not free-form strings. Entries must not contain secrets or raw search text.
/// </summary>
/// <param name="Timestamp">When the event happened.</param>
/// <param name="Level">The severity.</param>
/// <param name="Component">The component that wrote the entry.</param>
/// <param name="EventId">A stable identifier for the kind of event.</param>
/// <param name="CorrelationId">Links related entries, for example one activation or one search.</param>
/// <param name="Duration">How long the operation took, if it is a timed event.</param>
/// <param name="ErrorCode">A stable error code, if the entry describes a failure.</param>
/// <param name="Metadata">Extra safe key/value data.</param>
public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Component,
    string EventId,
    string? CorrelationId,
    TimeSpan? Duration,
    string? ErrorCode,
    IReadOnlyDictionary<string, string> Metadata);
