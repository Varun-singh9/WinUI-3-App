using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Launcher.Core.Diagnostics;

namespace Launcher.Diagnostics;

/// <summary>
/// Appends one JSON object per entry to a file. The file is bounded: when it grows past the limit it is moved to
/// <c>&lt;path&gt;.1</c>, replacing the previous rolled file, so at most two files exist. The file location is decided by
/// the data layer; this sink only receives a path.
/// </summary>
public sealed class JsonLinesFileSink : IDiagnosticsSink
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly object _gate = new();
    private readonly string _path;
    private readonly long _maxBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonLinesFileSink"/> class.
    /// </summary>
    /// <param name="path">The log file path. Its directory is created when needed.</param>
    /// <param name="maxBytes">The size above which the file is rolled before the next write.</param>
    public JsonLinesFileSink(string path, long maxBytes = 5 * 1024 * 1024)
    {
        _path = path;
        _maxBytes = maxBytes;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        var line = JsonSerializer.Serialize(entry, Options) + "\n";

        lock (_gate)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(_path));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(_path) && new FileInfo(_path).Length > _maxBytes)
            {
                File.Move(_path, _path + ".1", overwrite: true);
            }

            File.AppendAllText(_path, line, Utf8WithoutBom);
        }
    }
}
