using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using System.Dynamic;
using System.Net;

namespace ElsaWeb.Workflows;
//Details of workflow with ID:
public class HttpGetWorkflow : WorkflowBase
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
                    SupportedMethods = new(new[] { "GET" }),
                    Path = new("/api/workflows/{id}"),
                    CanStartWorkflow = true,
                    RouteData = new(routeParams)
                },
                // new SetVariable
                // {
                //     Variable = idVar,
                //     Value = new(context =>
                //         {
                //             var httpContextAccessor = context.GetRequiredService<IHttpContextAccessor>();
                //             var httpContext = httpContextAccessor.HttpContext!;
                //             var id = httpContextAccessor.HttpContext?.Request.RouteValues["id"]?.ToString();
                //             return id ?? "";
                //         })
                // },
                new WriteHttpResponse
                {
                    // Content = new(context => $"Details of workflow with ID: {idVar.Get(context)}")
                    Content = new(context =>
                    {
                        var id = routeParams.Get(context)?["id"]?.ToString() ?? "unknown";
                        return $"Workflow definition for ID: {id}";
                    })
                }
            }
        };
    }
}