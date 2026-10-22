namespace Launcher.Core.Actions;

/// <summary>
/// An executable action. Display and preview are separate from execution (Implementation Specification section 13).
/// </summary>
public interface IAction
{
    /// <summary>Gets the action identifier.</summary>
    string Id { get; }

    /// <summary>Gets the risk level of the action.</summary>
    RiskLevel Risk { get; }

    /// <summary>Describes the action for display without executing it.</summary>
    /// <param name="context">The context to describe the action for.</param>
    /// <returns>The display description.</returns>
    ActionDescriptor Describe(ActionContext context);

    /// <summary>Validates the arguments of the context without side effects.</summary>
    /// <param name="context">The context to validate.</param>
    /// <returns>The validation outcome.</returns>
    ActionValidation Validate(ActionContext context);

    /// <summary>Executes the action. Must only be called after successful validation and any required confirmation.</summary>
    /// <param name="context">The validated context.</param>
    /// <param name="cancellationToken">Cancels the execution.</param>
    /// <returns>The structured result.</returns>
    Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken);
}
