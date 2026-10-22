namespace Launcher.Core.Actions;

/// <summary>
/// Performs actions that search results describe. Uses the privileged broker only when an operation actually
/// requires elevation (Implementation Specification section 13).
/// </summary>
public interface IActionExecutor
{
    /// <summary>Validates and executes an action.</summary>
    /// <param name="context">The action context.</param>
    /// <param name="cancellationToken">Cancels the execution.</param>
    /// <returns>The structured result, including validation failures.</returns>
    Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken);
}
