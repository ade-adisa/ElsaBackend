using Elsa;
using Elsa.Extensions;
using Elsa.EntityFrameworkCore.Extensions;
// using Elsa.EntityFrameworkCore.DbContexts;
using ElsaWeb.Workflows;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddElsa(elsa =>
{
    // elsa.UseEntityFrameworkCorePersistence(ef => ef.UseSqlite("Data Source=elsa.db"));
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

//Avoid CORS Errors in Frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});


var app = builder.Build();

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