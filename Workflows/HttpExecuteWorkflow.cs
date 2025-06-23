using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using System.Dynamic;
using System.Net;

namespace ElsaWeb.Workflows;
// This simulates execution — once you hook into Elsa’s internal store/runtime, it will invoke actual definitions.
public class HttpExecuteWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        var idVar = builder.WithVariable<string>();
        var routeParams = builder.WithVariable<IDictionary<string, object>>();

        builder.Root = new Sequence
        {
            Activities =
            {
                new HttpEndpoint
                {
                    SupportedMethods = new(new[] { "POST" }),
                    Path = new("/api/workflows/{id}/execute"),
                    CanStartWorkflow = true,
                    QueryStringData = new(idVar),
                    RouteData = new(routeParams),
                },
                new WriteHttpResponse
                {
                    StatusCode = new(HttpStatusCode.OK),
                    Content = new(context =>
                    {
                        var id = routeParams.Get(context)?["id"]?.ToString() ?? "unknown";
                        var body = idVar.Get(context) ?? "{}"; //To get body if passed, not used now
                        return $"Executed workflow with ID: {id}";
                    })
                }
            }
        };
    }
}



// POST
// /api/workflows
// Save a new workflow definition
// POST
// /api/workflows/{id}/execute
// Execute workflow by ID
// GET
// /api/workflows
// List all workflow definitions
// GET
// /api/workflows/{id}
// Get a workflow definition by ID
