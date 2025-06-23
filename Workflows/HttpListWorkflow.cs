using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using System.Dynamic;
using System.Net;

namespace ElsaWeb.Workflows;
//List Workflows (GET /api/workflows) and Get by ID
public class HttpListWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        var idVar = builder.WithVariable<string>();

        builder.Root = new Sequence
        {
            Activities =
            {
                new HttpEndpoint
                {
                    SupportedMethods = new(new[] { "GET" }),
                    Path = new("/api/workflows"),
                    CanStartWorkflow = true
                },
                new WriteHttpResponse
                {
                    Content = new("Listing all workflows (simulated).")
                }
            }
        };
    }
}