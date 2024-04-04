using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


var app = builder.Build();

var sampleTodos = new Todo[] {
    new(1, "title 1","合格", DateOnly.FromDateTime(DateTime.Now.AddDays(-1))),
    new(2, "titlt 2","合格", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "title 3","失效", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "title 4","合格"),
    new(5, "title 5","失效", DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", (HttpRequest request) =>
{
    var query = sampleTodos.AsQueryable();
    if (request.Query.TryGetValue("Title", out var title))
        query = query.Where(t => t.Title.Contains(title));
    if (request.Query.TryGetValue("Result", out var result) && result!= "全部")
        query = query.Where(t => t.Result == result);
    
    return query.ToList().ToArray();
});
todosApi.MapGet("/title", () => TypedResults.Ok(sampleTodos.Select(t => t.Title).ToArray()));
app.UseCors(x => x
   .AllowAnyOrigin()
   .AllowAnyMethod()
   .AllowAnyHeader());

app.Urls.Add("http://localhost:5000");
app.Run();

public record Todo(int Id, string? Title, string? Result, DateOnly? DueDate = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(string[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

