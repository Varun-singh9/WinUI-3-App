using System.Text.Json;
using Launcher.Core.Diagnostics;
using Launcher.Diagnostics;
using Xunit;

namespace Launcher.Core.Tests;

/// <summary>
/// Behavior of the structured diagnostics implementation (Implementation Specification section 21).
/// </summary>
public sealed class DiagnosticsTests
{
    private static LogEntry Entry(LogLevel level = LogLevel.Information, IReadOnlyDictionary<string, string>? metadata = null)
    {
        return new LogEntry(
            DateTimeOffset.UnixEpoch,
            level,
            "Tests",
            "test.event",
            "corr-1",
            TimeSpan.FromMilliseconds(5),
            null,
            metadata ?? new Dictionary<string, string>());
    }

    [Fact]
    public void Entries_below_the_minimum_level_are_dropped()
    {
        var sink = new MemoryDiagnosticsSink();
        var service = new DiagnosticsService([sink], LogLevel.Warning);

        service.Write(Entry(LogLevel.Information));
        service.Write(Entry(LogLevel.Warning));
        service.Write(Entry(LogLevel.Error));

        Assert.Equal([LogLevel.Warning, LogLevel.Error], sink.Entries.Select(entry => entry.Level));
    }

    [Fact]
    public void A_failing_sink_does_not_throw_and_other_sinks_still_receive_the_entry()
    {
        var healthy = new MemoryDiagnosticsSink();
        var service = new DiagnosticsService([new ThrowingSink(), healthy]);

        var exception = Record.Exception(() => service.Write(Entry()));

        Assert.Null(exception);
        Assert.Single(healthy.Entries);
    }

    [Fact]
    public void Sensitive_metadata_values_are_redacted_and_other_values_are_kept()
    {
        var sink = new MemoryDiagnosticsSink();
        var service = new DiagnosticsService([sink]);
        var metadata = new Dictionary<string, string>
        {
            ["query"] = "my private search",
            ["apiToken"] = "abc123",
            ["providerId"] = "apps",
        };

        service.Write(Entry(metadata: metadata));

        var written = Assert.Single(sink.Entries).Metadata;
        Assert.Equal("[redacted]", written["query"]);
        Assert.Equal("[redacted]", written["apiToken"]);
        Assert.Equal("apps", written["providerId"]);
    }

    [Fact]
    public void The_file_sink_writes_one_json_object_per_line()
    {
        var directory = Directory.CreateTempSubdirectory("launcher-diagnostics-");
        try
        {
            var path = Path.Combine(directory.FullName, "nested", "diagnostics.jsonl");
            var service = new DiagnosticsService([new JsonLinesFileSink(path)]);

            service.Write(Entry());
            service.Write(Entry(LogLevel.Error));

            var lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);

            using var first = JsonDocument.Parse(lines[0]);
            Assert.Equal("Information", first.RootElement.GetProperty("Level").GetString());
            Assert.Equal("Tests", first.RootElement.GetProperty("Component").GetString());
            Assert.Equal("test.event", first.RootElement.GetProperty("EventId").GetString());
            Assert.False(first.RootElement.TryGetProperty("ErrorCode", out _));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void The_file_sink_rolls_the_file_when_it_grows_past_the_limit()
    {
        var directory = Directory.CreateTempSubdirectory("launcher-diagnostics-");
        try
        {
            var path = Path.Combine(directory.FullName, "diagnostics.jsonl");
            var sink = new JsonLinesFileSink(path, maxBytes: 200);

            for (var i = 0; i < 10; i++)
            {
                sink.Write(Entry());
            }

            Assert.True(File.Exists(path + ".1"));
            Assert.True(new FileInfo(path).Length <= 200 + 400);
            Assert.False(File.Exists(path + ".2"));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    private sealed class ThrowingSink : IDiagnosticsSink
    {
        public void Write(LogEntry entry)
        {
            throw new InvalidOperationException("sink failure");
        }
    }
}
