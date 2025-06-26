using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace ElsaWeb.Workflows;

public class HttpListWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
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
                    StatusCode = new(HttpStatusCode.OK),
                    Content = new(async context =>
                    {
                        var db = context.GetRequiredService<AppDbContext>();

                        var saved = await db.SavedWorkflows
                            .OrderByDescending(w => w.SavedAt)
                            .ToListAsync();

                        var transformed = saved.Select(w => new
                        {
                            id = w.WorkflowId,
                            name = w.Name,
                            definition = JsonSerializer.Deserialize<object>(w.DefinitionJson),
                            savedAt = w.SavedAt,
                        });

                        var json = JsonSerializer.Serialize(transformed, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                        return json;
                    }),
                    ContentType = new("application/json")
                }
            }
        };
    }
}