using Elsa;
using Elsa.Extensions;
using Elsa.EntityFrameworkCore.Extensions;
using ElsaWeb.Workflows;
using Microsoft.EntityFrameworkCore;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddElsa(elsa =>
{
    elsa.UseWorkflowManagement(runtime => runtime.UseEntityFrameworkCore(ef => ef.UseSqlite("Data Source=elsa.db")));
    elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef => ef.UseSqlite("Data Source=elsa.db")));
    elsa.AddWorkflow<HttpHelloWorld>();
    elsa.AddWorkflow<HttpSaveWorkflow>();
    elsa.AddWorkflow<HttpExecuteWorkflow>();
    elsa.AddWorkflow<HttpListWorkflow>();
    elsa.AddWorkflow<HttpGetWorkflow>();
    elsa.UseHttp(http => http.ConfigureHttpOptions = options =>
    {
        options.BaseUrl = new Uri("https://localhost:5039");
        options.BasePath = "/workflows";
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=elsa.db")); // Reuse the same DB

//Avoid CORS Errors in Frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});


var app = builder.Build();

app.UseRouting();
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering(); // Allow multiple reads
    await next();
});

// Auto-migrate database
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<ElsaDbContext>();
//     dbContext.Database.Migrate();
// }
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseWorkflows();
app.Run();

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SavedWorkflow> SavedWorkflows => Set<SavedWorkflow>();
}