using Launcher.Integrations.SingleInstance;
using Xunit;

namespace Launcher.Integration.Tests;

/// <summary>
/// Validation of the injected single-instance object names (ADR-0024).
/// </summary>
public sealed class SingleInstanceNamesTests
{
    [Fact]
    public void Default_names_are_valid_local_and_different()
    {
        var names = SingleInstanceNames.Default;

        Assert.StartsWith(@"Local\", names.MutexName, StringComparison.Ordinal);
        Assert.StartsWith(@"Local\", names.EventName, StringComparison.Ordinal);
        Assert.NotEqual(names.MutexName, names.EventName);
    }

    [Theory]
    [InlineData("", @"Local\event")]
    [InlineData(@"Local\mutex", " ")]
    [InlineData(@"Global\mutex", @"Local\event")]
    [InlineData(@"Local\mutex", @"Global\event")]
    [InlineData(@"Local\same", @"Local\same")]
    public void Invalid_names_are_rejected(string mutexName, string eventName)
    {
        Assert.Throws<ArgumentException>(() => new SingleInstanceNames(mutexName, eventName));
    }
}
