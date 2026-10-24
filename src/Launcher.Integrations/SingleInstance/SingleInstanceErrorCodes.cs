namespace Launcher.Integrations.SingleInstance;

/// <summary>
/// Stable, safe error codes reported by <see cref="SingleInstanceOwner"/> in results and diagnostics.
/// </summary>
internal static class SingleInstanceErrorCodes
{
    /// <summary>The operating system refused access to a single-instance object, for example one created with a restrictive ACL.</summary>
    public const string AccessDenied = "single_instance.access_denied";

    /// <summary>A single-instance object could not be created or opened.</summary>
    public const string CreateFailed = "single_instance.create_failed";

    /// <summary>The activation signal could not be sent to the running owner.</summary>
    public const string SignalFailed = "single_instance.signal_failed";

    /// <summary>A handler of the activation event threw.</summary>
    public const string HandlerFailed = "single_instance.handler_failed";
}
