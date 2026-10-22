namespace Launcher.PluginContracts;

/// <summary>
/// A request on the versioned plugin IPC protocol (Implementation Specification section 16). The receiver validates
/// the message, checks capabilities and enforces size limits and timeouts. Breaking changes need a new protocol version.
/// </summary>
/// <param name="RequestId">Identifies the request so its response can be correlated.</param>
/// <param name="ProtocolVersion">The protocol version the sender speaks.</param>
/// <param name="Operation">The operation to perform.</param>
/// <param name="Payload">The operation payload as schema-validated text. The payload encoding is fixed in the plugin phase.</param>
public sealed record IpcRequest(string RequestId, string ProtocolVersion, string Operation, string Payload);
