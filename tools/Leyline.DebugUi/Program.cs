using System.Text.Json.Serialization;
using Leyline.DebugUi;
using Leyline.RulesCore.Perception;
using LeylineHost = Leyline.Host;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var repoRoot = FindRepoRoot(builder.Environment.ContentRootPath);
builder.Services.AddSingleton(new GameSession(Path.Combine(repoRoot, "content", "cards")));

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

var scenariosDir = Path.Combine(app.Environment.ContentRootPath, "scenarios");

app.MapGet("/api/scenarios", () =>
    Directory.EnumerateFiles(scenariosDir, "*.scenario")
        .Select(Path.GetFileNameWithoutExtension)
        .OrderBy(name => name, StringComparer.Ordinal)
        .ToList());

app.MapPost("/api/scenarios/{name}/load", (string name, GameSession session) =>
{
    var path = Path.Combine(scenariosDir, name + ".scenario");
    if (!File.Exists(path))
        return Results.NotFound($"No scenario named '{name}'.");
    lock (session.Lock)
    {
        try
        {
            session.Load(name, path);
        }
        catch (Exception ex) when (ex is InvalidDataException or KeyNotFoundException)
        {
            return Results.BadRequest(ex.Message);
        }
    }
    return Results.Ok();
});

// Seat 1 = Champion A, 2 = Champion B, 0 = the omniscient true-state view — the debug UI's
// sanctioned exception to "never hand out true state" (architecture.md §2.9, risk §5.9).
app.MapGet("/api/view/{seat:int}", (int seat, GameSession session) =>
{
    lock (session.Lock)
    {
        if (session.State is null || session.Host is null)
            return Results.NotFound("No scenario loaded.");
        var view = seat == 0
            ? ViewProjector.Project(session.State, null, omniscient: true)
            : session.Host.CurrentView(new LeylineHost.SeatId(seat));
        return Results.Ok(new { view, scenario = session.ScenarioName, autoPass = session.AutoPass });
    }
});

app.MapGet("/api/legal/{seat:int}", (int seat, GameSession session) =>
{
    lock (session.Lock)
    {
        if (session.State is null || session.Host is null)
            return Results.NotFound("No scenario loaded.");
        var commands = session.Host.LegalCommands(new LeylineHost.SeatId(seat));
        return Results.Ok(commands.Select((c, i) => CommandDtos.ToDto(session.State, i, c)).ToList());
    }
});

app.MapPost("/api/submit", (SubmitRequest req, GameSession session) =>
{
    lock (session.Lock)
    {
        var result = session.Submit(req.Seat, req.Index);
        return Results.Ok(new SubmitResultDto(result.Accepted, result.Error));
    }
});

app.MapPost("/api/autopass", (AutoPassRequest req, GameSession session) =>
{
    lock (session.Lock)
    {
        session.AutoPass[req.Seat - 1] = req.Enabled;
        session.RunAutoPass();
        return Results.Ok();
    }
});

app.Run("http://localhost:5299");

static string FindRepoRoot(string start)
{
    var dir = new DirectoryInfo(start);
    while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "content", "cards")))
        dir = dir.Parent;
    return dir?.FullName ?? throw new DirectoryNotFoundException("content/cards not found above the debug UI.");
}
