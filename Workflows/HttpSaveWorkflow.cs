using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using System.Dynamic;
using System.Net;

namespace ElsaWeb.Workflows;
// This assumes a simple POST body (as JSON) and just echoes it back for now — next steps would integrate a proper store.
public class HttpSaveWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        var requestBody = builder.WithVariable<string>("requestBody");
        var userVariable = builder.WithVariable<ExpandoObject>();

        builder.Root = new Sequence
        {
            Activities =
            {
                new HttpEndpoint
                {
                    Path = new("/api/workflows"),
                    CanStartWorkflow = true,
                    SupportedMethods = new(new[] { "POST" }),
                    ParsedContent = new(userVariable),
                    QueryStringData = new(requestBody),
                },
                new WriteHttpResponse
                {
                    StatusCode = new(HttpStatusCode.OK),
                    Content = new(context =>
                    {
                        var body = requestBody.Get(context) ?? "{}";
                        return $"Workflow saved (mock): {body}";
                    })
                }
            }
        };
    }
}