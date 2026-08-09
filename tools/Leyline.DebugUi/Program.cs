using System.Text.Json.Serialization;
using Leyline.DebugUi;
using Leyline.Scenarios;
using LeylineHost = Leyline.Host;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSingleton<GameSession>();

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

    session.Load(ScenarioLoader.LoadFromFile(path));
    return Results.Ok();
});

app.MapGet("/api/view/{seat:int}", (int seat, GameSession session) =>
    session.Host is null
        ? Results.NotFound("No scenario loaded.")
        : Results.Ok(session.Host.CurrentView(new LeylineHost.SeatId(seat))));

app.MapGet("/api/legal/{seat:int}", (int seat, GameSession session) =>
{
    if (session.Host is null)
        return Results.NotFound("No scenario loaded.");

    var commands = session.Host.LegalCommands(new LeylineHost.SeatId(seat));
    var dto = commands.Select((c, i) => CommandLabeler.ToDto(i, c)).ToList();
    return Results.Ok(dto);
});

app.MapPost("/api/submit", (SubmitRequest req, GameSession session) =>
{
    if (session.Host is null)
        return Results.NotFound("No scenario loaded.");

    var seat = new LeylineHost.SeatId(req.Seat);
    var commands = session.Host.LegalCommands(seat);
    if (req.Index < 0 || req.Index >= commands.Count)
        return Results.Ok(new SubmitResultDto(false, "Stale or out-of-range action index — reload the view."));

    var result = session.Host.Submit(seat, commands[req.Index]);
    session.AutoResolvePriorityWindows(); // sorcery-speed-only for now — see GameSession's doc comment
    return Results.Ok(new SubmitResultDto(result.Accepted, result.RejectionReason));
});

app.MapGet("/api/truestate", (GameSession session) =>
    session.Match is null
        ? Results.NotFound("No scenario loaded.")
        : Results.Ok(DebugStateMapper.Map(session.Match.State)));

app.Run("http://localhost:5299");
