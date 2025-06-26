using Elsa.Workflows;
using Elsa.Workflows.Models;

public class Inline : Activity
{
    public required Func<ActivityExecutionContext, ValueTask> Action { get; init; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        await Action(context);
    }
}