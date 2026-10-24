using System.Security.AccessControl;
using System.Security.Principal;
using Launcher.Core.Diagnostics;
using Launcher.Core.Integrations;
using Launcher.Diagnostics;
using Launcher.Integrations.SingleInstance;
using Xunit;

namespace Launcher.Integration.Tests;

/// <summary>
/// Real named-mutex and named-event behavior of <see cref="SingleInstanceOwner"/> (ADR-0024). Every test class
/// instance uses its own unique object names, so these tests never clash with a running launcher or with each other.
/// </summary>
public sealed class SingleInstanceOwnerTests : IDisposable
{
    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(10);

    private readonly List<SingleInstanceOwner> _owners = [];
    private readonly MemoryDiagnosticsSink _sink = new();
    private readonly SingleInstanceNames _names = UniqueNames();

    public void Dispose()
    {
        foreach (var owner in _owners)
        {
            owner.Dispose();
        }
    }

    [Fact]
    public void The_first_instance_becomes_the_owner()
    {
        var owner = CreateOwner();

        var result = owner.TryAcquire();

        Assert.Equal(SingleInstanceStatus.Owner, result.Status);
        Assert.Null(result.ErrorCode);
        Assert.True(owner.IsOwner);
    }

    [Fact]
    public void A_second_instance_sees_the_first_one_running()
    {
        var first = CreateOwner();
        var second = CreateOwner();
        first.TryAcquire();

        var result = second.TryAcquire();

        Assert.Equal(SingleInstanceStatus.OtherInstanceRunning, result.Status);
        Assert.Null(result.ErrorCode);
        Assert.False(second.IsOwner);
        Assert.True(first.IsOwner);
    }

    [Fact]
    public void Acquiring_twice_keeps_ownership()
    {
        var owner = CreateOwner();

        var first = owner.TryAcquire();
        var second = owner.TryAcquire();

        Assert.Equal(SingleInstanceStatus.Owner, first.Status);
        Assert.Equal(SingleInstanceStatus.Owner, second.Status);
        Assert.True(owner.IsOwner);
    }

    [Fact]
    public void Releasing_lets_another_instance_become_the_owner()
    {
        var first = CreateOwner();
        var second = CreateOwner();
        first.TryAcquire();

        first.Release();
        var result = second.TryAcquire();

        Assert.False(first.IsOwner);
        Assert.Equal(SingleInstanceStatus.Owner, result.Status);
        Assert.True(second.IsOwner);
    }

    [Fact]
    public void Release_is_safe_when_not_owner_and_when_repeated()
    {
        var owner = CreateOwner();

        owner.Release();
        owner.TryAcquire();
        owner.Release();
        owner.Release();

        Assert.False(owner.IsOwner);
    }

    [Fact]
    public async Task A_signal_from_a_second_instance_raises_ActivationRequested_on_the_owner()
    {
        var owner = CreateOwner();
        owner.TryAcquire();
        var raised = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        owner.ActivationRequested += (_, _) => raised.TrySetResult();

        var second = CreateOwner();
        Assert.Equal(SingleInstanceStatus.OtherInstanceRunning, second.TryAcquire().Status);
        Assert.True(second.TrySignalOwner());

        await raised.Task.WaitAsync(WaitLimit, TestContext.Current.CancellationToken);
    }

    [Fact]
    public void A_signal_with_no_owner_reports_false()
    {
        var instance = CreateOwner();

        Assert.False(instance.TrySignalOwner());
    }

    [Fact]
    public void A_signal_after_the_owner_released_reports_false()
    {
        var owner = CreateOwner();
        owner.TryAcquire();
        owner.Release();

        var second = CreateOwner();

        Assert.False(second.TrySignalOwner());
    }

    [Fact]
    public async Task A_failing_handler_is_logged_without_its_message_and_later_signals_still_arrive()
    {
        var owner = CreateOwner();
        owner.TryAcquire();
        var calls = 0;
        var secondCallArrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        owner.ActivationRequested += (_, _) =>
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                throw new InvalidOperationException("secret handler message");
            }

            secondCallArrived.TrySetResult();
        };
        var sender = CreateOwner();

        Assert.True(sender.TrySignalOwner());
        await WaitUntilAsync(() => _sink.Entries.Any(entry => entry.ErrorCode == "single_instance.handler_failed"));
        Assert.True(sender.TrySignalOwner());
        await secondCallArrived.Task.WaitAsync(WaitLimit, TestContext.Current.CancellationToken);

        var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == "single_instance.handler_failed");
        Assert.Equal(LogLevel.Error, logged.Level);
        Assert.Equal("InvalidOperationException", logged.Metadata["exceptionType"]);
        Assert.DoesNotContain(logged.Metadata.Values, value => value.Contains("secret", StringComparison.Ordinal));
    }

    [Fact]
    public void The_signal_event_is_accessible_only_to_the_current_user()
    {
        var owner = CreateOwner();
        owner.TryAcquire();
        using var identity = WindowsIdentity.GetCurrent();
        var userSid = identity.User!.Value;

        using var signal = EventWaitHandleAcl.OpenExisting(_names.EventName, EventWaitHandleRights.ReadPermissions);
        var rules = signal.GetAccessControl()
            .GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
            .Cast<EventWaitHandleAccessRule>()
            .ToList();

        var rule = Assert.Single(rules);
        Assert.Equal(userSid, rule.IdentityReference.Value);
        Assert.Equal(AccessControlType.Allow, rule.AccessControlType);
    }

    [Fact]
    public void The_ownership_mutex_is_accessible_only_to_the_current_user()
    {
        var owner = CreateOwner();
        owner.TryAcquire();
        using var identity = WindowsIdentity.GetCurrent();
        var userSid = identity.User!.Value;

        using var mutex = MutexAcl.OpenExisting(_names.MutexName, MutexRights.ReadPermissions);
        var rules = mutex.GetAccessControl()
            .GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
            .Cast<MutexAccessRule>()
            .ToList();

        var rule = Assert.Single(rules);
        Assert.Equal(userSid, rule.IdentityReference.Value);
        Assert.Equal(AccessControlType.Allow, rule.AccessControlType);
    }

    [Fact]
    public void A_name_clash_with_another_kind_of_object_is_a_typed_logged_failure()
    {
        // Squat on the event's name with a mutex, so the owner cannot create its event.
        using var squatter = new Mutex(false, _names.EventName);
        var owner = CreateOwner();

        var result = owner.TryAcquire();

        Assert.Equal(SingleInstanceStatus.Failed, result.Status);
        Assert.Equal("single_instance.create_failed", result.ErrorCode);
        Assert.False(owner.IsOwner);
        var logged = Assert.Single(_sink.Entries, entry => entry.ErrorCode == "single_instance.create_failed");
        Assert.Equal(LogLevel.Error, logged.Level);
    }

    private static SingleInstanceNames UniqueNames()
    {
        var id = Guid.NewGuid().ToString("N");
        return new SingleInstanceNames($@"Local\Launcher.Tests.{id}.Instance", $@"Local\Launcher.Tests.{id}.Activate");
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + WaitLimit;
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "The condition was not met in time.");
            await Task.Delay(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }
    }

    private SingleInstanceOwner CreateOwner()
    {
        var owner = new SingleInstanceOwner(_names, new DiagnosticsService([_sink]));
        _owners.Add(owner);
        return owner;
    }
}
