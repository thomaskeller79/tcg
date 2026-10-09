using Leyline.Content.Json;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.Scenarios;

namespace Leyline.RulesCore.Tests.Support;

/// <summary>A match driven by tests through the real engine (RulesEngine.Apply), built from
/// scenario text and the repository's real card content (content/cards).</summary>
public sealed class Game
{
    public static readonly PlayerId A = PlayerId.A;
    public static readonly PlayerId B = PlayerId.B;

    private static readonly Lazy<ICardDefinitionRepository> LazyContent = new(() =>
    {
        var cards = CardJson.LoadDirectory(FindContentDir());
        return new CardDefinitionRepository(cards.All.Concat(TestCards.All));
    });

    public static ICardDefinitionRepository Content => LazyContent.Value;

    /// <summary>A small symmetric board: hexagon radius 3, Champion A at (0,2), B at (0,-2),
    /// one-hex home grounds, every other hex an Ember Field. Opening hand 0, no shuffle.</summary>
    public const string Base = """
        map hexagon 3
        start A 0,2
        start B 0,-2
        home A radius=0
        home B radius=0
        neutral fill terrain.fire
        champion A champion.pyra
        champion B champion.thorn
        terraindeck A terrain.fire
        terraindeck B terrain.earth
        openinghand 0
        noshuffle
        """;

    public TrueState State { get; }

    private Game(TrueState state) => State = state;

    public static Game Load(string extra = "", bool withBase = true) =>
        new(ScenarioLoader.Load((withBase ? Base + "\n" : "") + extra, Content));

    public static string FindContentDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "content", "cards");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("content/cards not found above the test output directory.");
    }

    // ---------------------------------------------------------------- finders

    public Permanent P(string card, int index = 0) =>
        State.Permanents.Where(p => p.Definition == card && p.Kind != PermanentKind.Remnant).ElementAtOrDefault(index)
        ?? throw new InvalidOperationException($"No permanent '{card}' #{index}.");

    public Permanent? Find(string card, int index = 0) =>
        State.Permanents.Where(p => p.Definition == card && p.Kind != PermanentKind.Remnant).ElementAtOrDefault(index);

    public Permanent Champion(PlayerId player) => State.ChampionOf(player)!;

    public Permanent Terrain(int q, int r) => State.TerrainOf(new HexCoord(q, r));

    public IReadOnlyList<Permanent> Remnants => State.Permanents.Where(p => p.Kind == PermanentKind.Remnant).ToList();

    public CardObject HandCard(PlayerId player, string card) =>
        State.Player(player).Hand.Select(State.Get<CardObject>).First(c => c.Definition == card);

    public static HexCoord H(int q, int r) => new(q, r);
    public static TargetChoice At(int q, int r, Slice? slice = null) => TargetChoice.ForLocation(new HexCoord(q, r), slice);
    public static TargetChoice Obj(GameObject o) => TargetChoice.ForObject(o.Id);
    public static TargetChoice AttackAt(int q, int r, Slice slice, PlayerId? entity) => TargetChoice.ForAttack(new HexCoord(q, r), slice, entity);

    // ---------------------------------------------------------------- actions

    public CommandResult Apply(Command command) => RulesEngine.Apply(State, command);

    /// <summary>Activates an ability; each target is its own one-element selection.</summary>
    public CommandResult Act(PlayerId actor, Permanent source, string ability, params TargetChoice[] targets) =>
        Activate(actor, source.Id, ability, targets.Select(t => (IReadOnlyList<TargetChoice>)[t]).ToList());

    public CommandResult ActRaw(PlayerId actor, GameObject source, string ability, IReadOnlyList<IReadOnlyList<TargetChoice>> targets) =>
        Activate(actor, source.Id, ability, targets);

    public CommandResult Cast(PlayerId actor, string card, params TargetChoice[] targets) =>
        Activate(actor, HandCard(actor, card).Id, DefaultAbilities.Cast, targets.Select(t => (IReadOnlyList<TargetChoice>)[t]).ToList());

    /// <summary>Where the payment needs a choice (D125), takes the first open one — tests that
    /// aren't about payment don't spell it out.</summary>
    private CommandResult Activate(PlayerId actor, ObjectId source, string ability, IReadOnlyList<IReadOnlyList<TargetChoice>> targets)
    {
        var command = new ActivateCommand(actor, source, ability, targets);
        if (Activation.Build(State, new ActivationRequest(actor, source, ability, targets)).Error != "Choose how to pay.")
            return Apply(command);
        var open = RulesEngine.LegalCommands(State, actor).OfType<ActivateCommand>().First(c => c.Source == source && c.Ability == ability);
        return Apply(command with { Mana = open.Mana });
    }

    public void Ok(CommandResult result)
    {
        Assert.True(result.Accepted, result.Error);
    }

    public void Pass(PlayerId actor) => Ok(Apply(new PassCommand(actor)));

    /// <summary>Whoever holds priority passes until Pending is empty again (no decision pending).</summary>
    public void ResolveAll()
    {
        for (var guard = 0; guard < 200 && State.Pending.Count > 0 && State.Decision is null && !State.IsOver; guard++)
            Pass(State.PriorityHolder!.Value);
    }

    /// <summary>Passes until the given seat's Action phase begins.</summary>
    public void AdvanceTo(Seat seat)
    {
        for (var guard = 0; guard < 400 && !State.IsOver; guard++)
        {
            if (State.ActiveSeat == seat && State.Phase == Phase.Action && State.Pending.Count == 0 && State.ConsecutivePasses == 0 && State.PriorityHolder == Rules.Turns.FirstChampion(State))
                return;
            if (State.Decision is not null)
                throw new InvalidOperationException("A decision is pending.");
            Pass(State.PriorityHolder!.Value);
        }
        throw new InvalidOperationException($"Never reached {seat}.");
    }

    /// <summary>Ends the current turn and plays on to the next turn of <paramref name="seat"/>.</summary>
    public void NextTurnOf(Seat seat)
    {
        var turn = State.TurnNumber;
        for (var guard = 0; guard < 400 && State.TurnNumber == turn && !State.IsOver; guard++)
            Pass(State.PriorityHolder!.Value);
        AdvanceTo(seat);
    }

    public IReadOnlyList<string> Log => State.Log.Select(l => l.Text).ToList();
}

/// <summary>Cards defined only for tests.</summary>
public static class TestCards
{
    public static readonly CardDefinition Grunt = new()
    {
        Id = "test.grunt", Name = "Grunt", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 3, Life = 5, Ap = 4,
    };

    public static readonly CardDefinition Mirror = new()
    {
        Id = "test.mirror", Name = "Mirror Knight", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 3, Life = 5, Ap = 4,
    };

    public static readonly CardDefinition Brute = new()
    {
        Id = "test.brute", Name = "Test Brute", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 3, Life = 7, Ap = 4,
    };

    public static readonly CardDefinition Mole = new()
    {
        Id = "test.mole", Name = "Test Mole", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 1, Life = 3, Ap = 6,
        Keywords = [new Keyword(Keyword.Subterranean)],
    };

    public static readonly CardDefinition Knotter = new()
    {
        Id = "test.knotter", Name = "Knotter", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 1, Life = 3, Ap = 4,
        Keywords = [new Keyword(Keyword.Knotting)],
    };

    /// <summary>A Subterranean creature whose Slice filter names Root — the one way a cast can
    /// meet a hidden occupant (D68).</summary>
    public static readonly CardDefinition Digger = new()
    {
        Id = "test.digger", Name = "Digger", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 1, Life = 2, Ap = 3,
        Keywords = [new Keyword(Keyword.Subterranean)], EntersSlice = Slice.Root,
    };

    /// <summary>D128: a Beginning trigger at a stated Speed.</summary>
    public static CardDefinition DawnSeer(Speed speed) => new()
    {
        Id = $"test.dawn-seer-{speed.ToString().ToLowerInvariant()}", Name = "Dawn Seer", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 1, Life = 3, Ap = 4,
        Abilities = [new AbilityDefinition { Id = "dawn-draw", Name = "Dawn Draw", Trigger = TriggerEvent.BeginningOfYourTurn, Speed = speed, Instructions = [new Instruction("draw", Amount: 1)] }],
    };

    public static IEnumerable<CardDefinition> All => [Grunt, Mirror, Brute, Mole, Knotter, Digger, DawnSeer(Speed.Reactive), DawnSeer(Speed.Instant)];
}
