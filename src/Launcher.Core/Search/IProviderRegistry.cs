namespace Launcher.Core.Search;

/// <summary>
/// Holds the registered providers. Registration never blocks critical UI activation (Implementation Specification section 5).
/// </summary>
public interface IProviderRegistry
{
    /// <summary>Gets all registered providers.</summary>
    IReadOnlyList<ISearchProvider> Providers { get; }

    /// <summary>Registers a provider.</summary>
    /// <param name="provider">The provider to register.</param>
    void Register(ISearchProvider provider);

    /// <summary>Finds a provider by identifier.</summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="provider">The provider when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when a provider with that identifier is registered; otherwise <see langword="false"/>.</returns>
    bool TryGet(string providerId, out ISearchProvider? provider);
}
