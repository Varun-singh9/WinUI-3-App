namespace Launcher.Data;

/// <summary>
/// Where the launcher keeps its per-user data on disk (ADR-0020, ADR-0028). This type only computes paths. It creates
/// no folders: the file that writes into a folder, such as the diagnostics file sink, creates it when needed.
/// </summary>
/// <remarks>
/// All data lives under one root, so a later "reset application data" has a single place to act on. The folder name
/// and the sub-folder and file names are part of where users' data lives, so changing them later needs a migration
/// once configuration or a database is stored here.
/// </remarks>
public sealed class DataLocations
{
    /// <summary>The default name of the per-user data folder. Provisional: the documents settle no product name (ADR-0028).</summary>
    public const string DefaultFolderName = "Launcher";

    /// <summary>The name of the folder under the root that holds log files.</summary>
    public const string LogsFolderName = "logs";

    /// <summary>The name of the diagnostics log file.</summary>
    public const string LogFileName = "launcher.log";

    // The characters Windows does not allow in a file or folder name. Listed explicitly, not taken from the current
    // operating system, so the rule is the same wherever the code runs.
    private static readonly char[] InvalidNameCharacters = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    /// <summary>
    /// Initializes a new instance of the <see cref="DataLocations"/> class.
    /// </summary>
    /// <param name="rootDirectory">The absolute path of the data root. It is normalized and a trailing separator is removed.</param>
    /// <exception cref="ArgumentException">The path is blank or is not fully qualified.</exception>
    public DataLocations(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        if (!Path.IsPathFullyQualified(rootDirectory))
        {
            throw new ArgumentException("The data root must be a fully qualified path.", nameof(rootDirectory));
        }

        RootDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));
    }

    /// <summary>Gets the data root.</summary>
    public string RootDirectory { get; }

    /// <summary>Gets the folder that holds log files.</summary>
    public string LogsDirectory => Path.Combine(RootDirectory, LogsFolderName);

    /// <summary>Gets the full path of the diagnostics log file.</summary>
    public string LogFilePath => Path.Combine(LogsDirectory, LogFileName);

    /// <summary>
    /// Creates the locations for the current user, under the per-user local application data folder.
    /// </summary>
    /// <param name="folderName">The name of the data folder under it.</param>
    /// <returns>The locations.</returns>
    /// <exception cref="ArgumentException">The folder name is blank, contains a path separator or another character Windows does not allow, is a relative segment, or ends with a dot or a space.</exception>
    /// <exception cref="InvalidOperationException">The per-user local application data folder is not available.</exception>
    public static DataLocations ForCurrentUser(string folderName = DefaultFolderName)
    {
        ValidateFolderName(folderName);

        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localApplicationData))
        {
            throw new InvalidOperationException("The per-user local application data folder is not available.");
        }

        return new DataLocations(Path.Combine(localApplicationData, folderName));
    }

    private static void ValidateFolderName(string folderName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderName);

        if (folderName is "." or ".."
            || folderName.IndexOfAny(InvalidNameCharacters) >= 0
            || folderName.Any(char.IsControl)
            || folderName.EndsWith('.')
            || folderName.EndsWith(' '))
        {
            throw new ArgumentException("The folder name is not a valid single folder name.", nameof(folderName));
        }
    }
}
