using System.Net;
using System.Text.Json.Serialization;
using Elsa.Extensions;
using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Microsoft.AspNetCore.Http;
using Elsa.Workflows.Models;

namespace ElsaWeb.Workflows;

public class WorkflowSaveRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("definition")]
    public object Definition { get; set; } = default!;
}


public class SavedWorkflow
{
    public Guid Id { get; set; }
    public string WorkflowId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string DefinitionJson { get; set; } = default!;
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

public class HttpSaveWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        var requestVar = builder.WithVariable<WorkflowSaveRequest>();

        builder.Root = new Sequence
        {
            Activities =
            {
                new HttpEndpoint
                {
                    Path = new("/save"),
                    CanStartWorkflow = true,
                    SupportedMethods = new(new[] { "POST" })
                },
                new SetVariable
                {
                    Variable = requestVar,
                    Value = new(context =>
                    {
                        try
                        {
                            var accessor = context.GetRequiredService<IHttpContextAccessor>();
                            var request = accessor.HttpContext?.Request;

                            if (request == null || request.ContentLength == 0)
                                return new WorkflowSaveRequest();

                            request.EnableBuffering();
                            request.Body.Position = 0;

                            using var reader = new StreamReader(request.Body, leaveOpen: true);
                            var json = reader.ReadToEndAsync().GetAwaiter().GetResult();
                            request.Body.Position = 0;

                            var data = System.Text.Json.JsonSerializer.Deserialize<WorkflowSaveRequest>(json);
                            Console.WriteLine($"📦 Parsed ID: {data?.Id}, Name: {data?.Name}");
                            return data ?? new WorkflowSaveRequest();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("❌ Deserialization error: " + ex.Message);
                            Console.WriteLine(ex.StackTrace);
                            return new WorkflowSaveRequest(); // fallback
                        }
                    })
                },
                new Inline
                {
                    Action = async context =>
                    {
                        var req = requestVar.Get(context);
                        var db = context.GetRequiredService<AppDbContext>();

                        var record = new SavedWorkflow
                        {
                            Id = Guid.NewGuid(),
                            WorkflowId = req.Id,
                            Name = req.Name,
                            DefinitionJson = System.Text.Json.JsonSerializer.Serialize(req.Definition),
                            SavedAt = DateTime.UtcNow
                        };

                        db.SavedWorkflows.Add(record);
                        await db.SaveChangesAsync();
                        Console.WriteLine($"✅ Saved: {record.Name} ({record.WorkflowId})");
                    }
                },
                new WriteHttpResponse
                {
                    StatusCode = new(HttpStatusCode.OK),
                    Content = new(context =>
                    {
                        var req = requestVar.Get(context);
                        return $"✅ Saved workflow '{req.Name}' with ID: {req.Id}";
                    })
                }
            }
        };
    }
}