namespace Launcher.PluginContracts;

/// <summary>
/// A response on the versioned plugin IPC protocol, correlated to a request by <see cref="RequestId"/>.
/// </summary>
/// <param name="RequestId">The identifier of the request being answered.</param>
/// <param name="Status">The outcome.</param>
/// <param name="ErrorCode">A stable, safe error code when the outcome is not success.</param>
/// <param name="Payload">The result payload, if any. The payload encoding is fixed in the plugin phase.</param>
public sealed record IpcResponse(string RequestId, IpcStatus Status, string? ErrorCode, string? Payload);
