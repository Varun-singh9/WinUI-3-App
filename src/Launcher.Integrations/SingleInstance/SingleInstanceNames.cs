namespace Launcher.Integrations.SingleInstance;

/// <summary>
/// The kernel object names used for single-instance ownership and the activation signal (ADR-0024). The names are
/// injected so tests can use unique names and never clash with a running launcher or with each other.
/// </summary>
public sealed class SingleInstanceNames
{
    private const string LocalPrefix = @"Local\";

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleInstanceNames"/> class.
    /// </summary>
    /// <param name="mutexName">The name of the ownership mutex. It must start with <c>Local\</c>.</param>
    /// <param name="eventName">The name of the activation signal event. It must start with <c>Local\</c> and differ from the mutex name.</param>
    /// <exception cref="ArgumentException">A name is blank, is not in the <c>Local\</c> namespace, or both names are the same.</exception>
    public SingleInstanceNames(string mutexName, string eventName)
    {
        MutexName = Validate(mutexName, nameof(mutexName));
        EventName = Validate(eventName, nameof(eventName));

        if (string.Equals(MutexName, EventName, StringComparison.Ordinal))
        {
            throw new ArgumentException("The mutex name and the event name must be different.", nameof(eventName));
        }
    }

    /// <summary>Gets the names used by the launcher itself.</summary>
    public static SingleInstanceNames Default { get; } = new(@"Local\Launcher.V3.Instance", @"Local\Launcher.V3.Instance.Activate");

    /// <summary>Gets the name of the ownership mutex.</summary>
    public string MutexName { get; }

    /// <summary>Gets the name of the activation signal event.</summary>
    public string EventName { get; }

    private static string Validate(string name, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, parameterName);

        // The Local namespace is per logon session, matching where a global hotkey and the launcher window live.
        // Global names are refused so a name can never accidentally be shared across sessions.
        if (!name.StartsWith(LocalPrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException(@"The name must start with ""Local\"".", parameterName);
        }

        return name;
    }
}
