namespace Launcher.Core.Integrations;

/// <summary>
/// Owns single-instance ownership for the current user and carries the "activate the running instance" signal
/// (Implementation Specification sections 5 and 6, ADR-0024). The signal has no payload: a second instance can ask
/// the owner to activate, and nothing else. The implementation lives in <c>Launcher.Integrations</c>.
/// Expected failures are returned as values and never thrown.
/// </summary>
public interface ISingleInstanceOwner
{
    /// <summary>
    /// Raised on the owner when another instance asks it to activate. It may be raised on a background thread, so
    /// handlers must marshal to their own context and must not block.
    /// </summary>
    event EventHandler? ActivationRequested;

    /// <summary>Gets a value indicating whether this process currently owns single-instance ownership.</summary>
    bool IsOwner { get; }

    /// <summary>
    /// Tries to become the owner. While this process is the owner, <see cref="ActivationRequested"/> is raised when
    /// another instance signals.
    /// </summary>
    /// <returns>The typed outcome.</returns>
    SingleInstanceResult TryAcquire();

    /// <summary>Asks the running owner to activate. Used by a second instance before it exits.</summary>
    /// <returns><see langword="true"/> when the signal reached a running owner; otherwise <see langword="false"/>.</returns>
    bool TrySignalOwner();

    /// <summary>Gives up ownership, if held. Safe to call when this process is not the owner.</summary>
    void Release();
}
