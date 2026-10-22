namespace Launcher.Core.Search;

/// <summary>
/// The single source of truth for available prefixes. The parser and the prefix popup must use the same registry
/// (Implementation Specification section 10).
/// </summary>
public interface IPrefixRegistry
{
    /// <summary>Gets all registered prefixes.</summary>
    IReadOnlyList<PrefixDefinition> Prefixes { get; }

    /// <summary>Resolves a typed text or symbol alias to a prefix.</summary>
    /// <param name="alias">The alias as typed.</param>
    /// <param name="prefix">The matching prefix when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when the alias is known; otherwise <see langword="false"/>.</returns>
    bool TryResolve(string alias, out PrefixDefinition? prefix);
}
