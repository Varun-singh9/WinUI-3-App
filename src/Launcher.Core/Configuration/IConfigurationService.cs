namespace Launcher.Core.Configuration;

/// <summary>
/// The only path to persistent settings. UI controls never write directly to storage
/// (Implementation Specification section 18). Secrets never live in ordinary configuration.
/// </summary>
public interface IConfigurationService
{
    /// <summary>Gets a typed settings section.</summary>
    /// <typeparam name="T">The settings section type.</typeparam>
    /// <returns>The current value of the section.</returns>
    T Get<T>()
        where T : class;

    /// <summary>Replaces a typed settings section.</summary>
    /// <typeparam name="T">The settings section type.</typeparam>
    /// <param name="value">The new value.</param>
    void Set<T>(T value)
        where T : class;

    /// <summary>Observes changes to a typed settings section.</summary>
    /// <typeparam name="T">The settings section type.</typeparam>
    /// <returns>A sequence that produces the section whenever it changes.</returns>
    IObservable<T> Observe<T>()
        where T : class;

    /// <summary>Validates the current configuration.</summary>
    /// <returns>The validation outcome.</returns>
    ConfigurationValidationResult Validate();

    /// <summary>Exports the configuration without secrets.</summary>
    /// <param name="destination">The stream to write to.</param>
    /// <param name="cancellationToken">Cancels the export.</param>
    /// <returns>A task that completes when the export has finished.</returns>
    Task ExportAsync(Stream destination, CancellationToken cancellationToken);

    /// <summary>Imports configuration after validating it.</summary>
    /// <param name="source">The stream to read from.</param>
    /// <param name="cancellationToken">Cancels the import.</param>
    /// <returns>A task that completes when the import has finished.</returns>
    Task ImportAsync(Stream source, CancellationToken cancellationToken);

    /// <summary>Migrates the configuration format, creating a recoverable backup first.</summary>
    /// <param name="cancellationToken">Cancels the migration.</param>
    /// <returns>A task that completes when the migration has finished.</returns>
    Task MigrateAsync(CancellationToken cancellationToken);
}
