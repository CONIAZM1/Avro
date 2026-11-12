using Avro;
using AvroSpike.Core;
using System.Collections.Concurrent;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// The API is independent of Contracts: it can start BEFORE generating the types.
string source = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "Schemas"));
if (!Directory.Exists(source))
{
    source = Path.Combine(AppContext.BaseDirectory, "Schemas");
}

var catalog = new SchemaCatalog(source);
var requests = new ConcurrentQueue<object>();
WebApplication app = builder.Build();

// Logging requests demonstrates that EVERY build queries the API again.
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    if (context.Request.Path.StartsWithSegments("/schemas"))
    {
        var entry = new { utc = DateTime.UtcNow, path = context.Request.Path.ToString() };
        requests.Enqueue(entry);
        Console.WriteLine($"BUILD REQUEST: {entry.path} at {entry.utc:O}");
    }
    await next();
});

app.MapGet("/", () => Results.Ok(new
{
    application = "AvroSpike schema catalog",
    sourceOfTruth = source,
    versions = new[] { 1, 2, 3 },
    endpoints = new[] { "/schemas/policy/1/avsc", "/schemas/policy/1/csharp", "/build-requests" }
}));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/schemas/policy/{version:int}/avsc", (int version) =>
{
    return version is < 1 or > 3 ? Results.NotFound() : Results.Text(catalog.Read(version), "application/json");
});
app.MapGet("/schemas/policy/{version:int}/csharp", (int version) =>
{
    if (version is < 1 or > 3)
    {
        return Results.NotFound(); // this could be improved... I know
    }
    // the C# POCOS are generated in the Core project, but the API is independent of Contracts: it can start BEFORE generating the types.
    // actually it has to start BEFORE generating the types, because it is used DURING the build to generate the types.
    // what a time 
    return Results.Text(SchemaCatalog.GenerateCSharp(catalog.Read(version)), "text/plain");
});
app.MapGet("/versions", () => Enumerable.Range(1, 3).Select(version =>
{
    var schema = (RecordSchema)Schema.Parse(catalog.Read(version));
    return new { version, fullname = schema.Fullname, properties = schema.Fields.Select(f => f.Name) };
}));
app.MapGet("/build-requests", () => new { total = requests.Count, requests = requests.ToArray() });
app.Run("http://localhost:5185");
