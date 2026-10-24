using Launcher.Core.Integrations;
using Launcher.Diagnostics;
using Launcher.Integrations.Hotkey;
using Xunit;

namespace Launcher.Integration.Tests;

/// <summary>
/// Tests that register real global hotkeys with Windows (ADR-0024). They use an unlikely gesture, always release it
/// when they finish, and skip with an explicit reason when registration is not possible on this machine or in CI.
/// A skip is not a pass: the result summary shows it as skipped.
/// </summary>
public sealed class RealHotkeyTests : IDisposable
{
    // Control+Alt+Shift+F24: a key that practically no application binds.
    private static readonly HotkeyGesture Unlikely = new(HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift, 0x87);

    private readonly List<HotkeyService> _services = [];
    private readonly MemoryDiagnosticsSink _sink = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        // Disposing releases the registration, so no test leaves a hotkey behind even when it fails.
        foreach (var service in _services)
        {
            service.Dispose();
        }
    }

    [Fact]
    public async Task A_real_registration_succeeds_and_is_released()
    {
        var service = Create();
        var result = await service.RegisterAsync(Unlikely, Token);
        SkipUnlessRegistered(result);

        Assert.Equal(HotkeyRegistrationStatus.Registered, result.Status);
        Assert.Equal(Unlikely, service.Current);

        await service.UnregisterAsync(Token);

        Assert.Null(service.Current);

        // The gesture is free again: another service can take it.
        var other = Create();
        var again = await other.RegisterAsync(Unlikely, Token);
        Assert.Equal(HotkeyRegistrationStatus.Registered, again.Status);
    }

    [Fact]
    public async Task A_real_conflict_is_reported_as_in_use()
    {
        var first = Create();
        var firstResult = await first.RegisterAsync(Unlikely, Token);
        SkipUnlessRegistered(firstResult);

        var second = Create();
        var secondResult = await second.RegisterAsync(Unlikely, Token);

        Assert.Equal(HotkeyRegistrationStatus.InUse, secondResult.Status);
        Assert.Equal("hotkey.in_use", secondResult.ErrorCode);
        Assert.Null(second.Current);
        Assert.Equal(Unlikely, first.Current);
    }

    private static void SkipUnlessRegistered(HotkeyRegistrationResult result)
    {
        if (result.Status != HotkeyRegistrationStatus.Registered)
        {
            Assert.Skip(
                $"A real global hotkey could not be registered here ({result.Status}, {result.ErrorCode}). " +
                "Another application may own the gesture, or this session has no interactive desktop.");
        }
    }

    private HotkeyService Create()
    {
        var service = new HotkeyService(new DiagnosticsService([_sink]));
        _services.Add(service);
        return service;
    }
}
