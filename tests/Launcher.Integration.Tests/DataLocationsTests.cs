using Launcher.Data;
using Xunit;

namespace Launcher.Integration.Tests;

/// <summary>
/// Path composition and validation of <see cref="DataLocations"/> (ADR-0028). These tests never create a folder,
/// because the type only computes paths.
/// </summary>
public sealed class DataLocationsTests
{
    private static string UniqueRoot()
    {
        return Path.Combine(Path.GetTempPath(), "LauncherDataLocationsTests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public void The_root_logs_folder_and_log_file_paths_are_composed_from_the_root()
    {
        var root = UniqueRoot();

        var locations = new DataLocations(root);

        Assert.Equal(root, locations.RootDirectory);
        Assert.Equal(Path.Combine(root, "logs"), locations.LogsDirectory);
        Assert.Equal(Path.Combine(root, "logs", "launcher.log"), locations.LogFilePath);
    }

    [Fact]
    public void A_trailing_separator_is_removed_from_the_root()
    {
        var root = UniqueRoot();

        var locations = new DataLocations(root + Path.DirectorySeparatorChar);

        Assert.Equal(root, locations.RootDirectory);
    }

    [Fact]
    public void A_root_with_dot_dot_segments_is_normalized()
    {
        var root = UniqueRoot();

        var locations = new DataLocations(Path.Combine(root, "a", "..", "b"));

        Assert.Equal(Path.Combine(root, "b"), locations.RootDirectory);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("relative")]
    [InlineData(@"relative\logs")]
    [InlineData("C:logs")]
    [InlineData(@"\logs")]
    public void A_root_that_is_blank_or_not_fully_qualified_is_rejected(string root)
    {
        Assert.Throws<ArgumentException>(() => new DataLocations(root));
    }

    [Fact]
    public void The_default_folder_for_the_current_user_is_under_local_application_data()
    {
        var locations = DataLocations.ForCurrentUser();

        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Launcher");
        Assert.Equal(Path.TrimEndingDirectorySeparator(expected), locations.RootDirectory);
    }

    [Fact]
    public void A_chosen_valid_folder_name_is_used()
    {
        var locations = DataLocations.ForCurrentUser("My Launcher 2");

        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "My Launcher 2");
        Assert.Equal(expected, locations.RootDirectory);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData(@"a\b")]
    [InlineData("a/b")]
    [InlineData("bad:name")]
    [InlineData("pipe|name")]
    [InlineData("ends.with.dot.")]
    [InlineData("ends with space ")]
    public void An_invalid_folder_name_is_rejected(string folderName)
    {
        Assert.Throws<ArgumentException>(() => DataLocations.ForCurrentUser(folderName));
    }

    [Fact]
    public void The_default_names_are_pinned_because_changing_them_moves_user_data()
    {
        Assert.Equal("Launcher", DataLocations.DefaultFolderName);
        Assert.Equal("logs", DataLocations.LogsFolderName);
        Assert.Equal("launcher.log", DataLocations.LogFileName);
    }

    [Fact]
    public void Computing_the_paths_creates_nothing_on_disk()
    {
        var root = UniqueRoot();

        var locations = new DataLocations(root);
        _ = locations.LogsDirectory;
        _ = locations.LogFilePath;

        Assert.False(Directory.Exists(root));
    }
}
