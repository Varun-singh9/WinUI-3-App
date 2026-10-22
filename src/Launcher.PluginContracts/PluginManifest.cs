namespace Launcher.PluginContracts;

/// <summary>
/// A plugin's declared identity, capabilities and protocol version. Canonical fields per the V2.7 clarification:
/// <c>Version</c> identifies the plugin package, <c>ApiVersion</c> the stable plugin contract and
/// <c>ProtocolVersion</c> the IPC message shape. The historical names sdkVersion and pluginVersion are superseded.
/// </summary>
/// <param name="Id">The stable plugin identifier.</param>
/// <param name="Name">The user-visible name.</param>
/// <param name="Version">The plugin package version.</param>
/// <param name="ApiVersion">The version of the plugin contract the plugin targets.</param>
/// <param name="ProtocolVersion">The IPC message-shape version the plugin speaks.</param>
/// <param name="Publisher">Who publishes the plugin.</param>
/// <param name="Capabilities">The capability identifiers the plugin requests. Default deny applies and the plugin cannot expand them.</param>
/// <param name="EntryPoint">How the plugin process is started.</param>
/// <param name="Providers">The provider identifiers the plugin supplies.</param>
/// <param name="Commands">The command identifiers the plugin supplies.</param>
/// <param name="Settings">The setting identifiers the plugin declares.</param>
/// <param name="MinimumLauncherVersion">The oldest launcher version the plugin supports.</param>
public sealed record PluginManifest(
    string Id,
    string Name,
    string Version,
    string ApiVersion,
    string ProtocolVersion,
    string Publisher,
    IReadOnlyList<string> Capabilities,
    string EntryPoint,
    IReadOnlyList<string> Providers,
    IReadOnlyList<string> Commands,
    IReadOnlyList<string> Settings,
    string MinimumLauncherVersion);
