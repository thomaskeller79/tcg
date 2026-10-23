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
    /// meet a hidden occupant (D132).</summary>
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

    /// <summary>D133: a Structure a card grants Defend.</summary>
    public static readonly CardDefinition Bastion = new()
    {
        Id = "test.bastion", Name = "Bastion", Type = CardType.Structure, Cost = new Cost([ManaPip.Generic]), Life = 6, Ap = 2,
        Abilities = [DefaultAbilities.DefendAbility],
    };

    /// <summary>D129: a terrain slowing Move abilities of permanents standing on it (leaving).</summary>
    public static readonly CardDefinition Bog = new()
    {
        Id = "test.bog", Name = "Bog", Type = CardType.Terrain, Produces = [ManaUnit.Parse("Water")],
        Statics = [new StaticAbility("Move", StaticScope.OnThis, 2)],
    };

    /// <summary>D129: a printed ability of subtype Move, which Move surcharges reach.</summary>
    public static readonly CardDefinition Leaper = new()
    {
        Id = "test.leaper", Name = "Leaper", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 1, Life = 3, Ap = 6,
        Abilities = [new AbilityDefinition { Id = "leap", Name = "Leap", Subtypes = ["Move"], Cost = Cost.ApOnly(1), Instructions = [new Instruction("quicken", Target: "self", Amount: 0)] }],
    };

    /// <summary>D135: an exhausting (`!`) Move-subtype ability with a target, which surcharges reach.</summary>
    public static readonly CardDefinition Vaulter = new()
    {
        Id = "test.vaulter", Name = "Vaulter", Type = CardType.Creature, Cost = new Cost([ManaPip.Generic]), Attack = 1, Life = 3, Ap = 4,
        Abilities = [new AbilityDefinition { Id = "vault", Name = "Vault", Subtypes = ["Move"], Cost = Cost.ApOnly(1, ApFlavor.Exhaust), Targets = [new TargetSpec("T", TargetKind.Terrain)], Instructions = [new Instruction("quicken", Target: "self", Amount: 0)] }],
    };

    /// <summary>D143/D144: +2 Health and +2 AP until the end of this turn (Duration 1).</summary>
    public static readonly CardDefinition Fortify = new()
    {
        Id = "test.fortify", Name = "Fortify", Type = CardType.Spell, Speed = Speed.Reactive, Duration = 1,
        Targets = [new TargetSpec("T1", TargetKind.Creature)],
        Instructions = [new Instruction("health", "T1", 2), new Instruction("ap", "T1", 2)],
    };

    /// <summary>Free spells, one per new instruction of D141 (Track B item 23).</summary>
    private static CardDefinition Spell(string id, Speed speed, TargetSpec[] targets, params Instruction[] instructions) => new()
    {
        Id = id, Name = id, Type = CardType.Spell, Speed = speed, Duration = 1, Targets = targets, Instructions = instructions,
    };

    public static IEnumerable<CardDefinition> Instructions =>
    [
        Spell("test.discard", Speed.Slow, [new TargetSpec("T1", TargetKind.Card, Section: "Hand")], new Instruction("discard", "T1")),
        Spell("test.mill", Speed.Slow, [new TargetSpec("T1", TargetKind.Creature)], new Instruction("mill", "T1", 2)),
        Spell("test.shuffle", Speed.Slow, [], new Instruction("shuffle", Section: "Library")),
        Spell("test.stash", Speed.Slow, [new TargetSpec("T1", TargetKind.Card, ControlFilter.You, Section: "Hand")], new Instruction("move", "T1", Section: "Library")),
        Spell("test.darkness", Speed.Reactive, [new TargetSpec("T1", TargetKind.Terrain)], new Instruction("darken", "T1", Section: "Ground")),
        Spell("test.recover", Speed.Slow, [new TargetSpec("T1", TargetKind.Card, ControlFilter.You, Section: "Discard")], new Instruction("move", "T1", Section: "Hand")),
        Spell("test.counter", Speed.Reactive, [new TargetSpec("T1", TargetKind.Trace, Section: "Pending")], new Instruction("destroy", "T1")),
        Spell("test.echo", Speed.Reactive, [new TargetSpec("T1", TargetKind.Trace, Section: "Pending")], new Instruction("copy", "T1")),
        Spell("test.twin", Speed.Slow, [new TargetSpec("T1", TargetKind.Creature), new TargetSpec("T2", TargetKind.Terrain, ControlFilter.You)], new Instruction("copy", "T1", To: "T2")),
        Spell("test.duplicate", Speed.Slow, [new TargetSpec("T1", TargetKind.Card, ControlFilter.You, Section: "Hand")], new Instruction("copy", "T1")),
        Spell("test.teleport", Speed.Slow, [new TargetSpec("T1", TargetKind.Creature), new TargetSpec("T2", TargetKind.Terrain)], new Instruction("move", "T1", To: "T2")),
        Spell("test.wings", Speed.Slow, [new TargetSpec("T1", TargetKind.Creature)], new Instruction("gain", "T1", Ability: "Flying")),
        Spell("test.rooted", Speed.Slow, [new TargetSpec("T1", TargetKind.Creature)], new Instruction("lose", "T1", Ability: "move")),
        Spell("test.ritual", Speed.Slow, [], new Instruction("add mana", Amount: 3, Mana: ManaUnit.Of(Element.Darkness))),
        Spell("test.dominate", Speed.Slow, [new TargetSpec("T1", TargetKind.Creature)], new Instruction("gain control", "T1")),
        Spell("test.arm", Speed.Slow, [new TargetSpec("T1", TargetKind.Item), new TargetSpec("T2", TargetKind.Creature)], new Instruction("attach", "T1", To: "T2")),
        Spell("test.disarm", Speed.Slow, [new TargetSpec("T1", TargetKind.Item)], new Instruction("detach", "T1")),
    ];

    /// <summary>D150 Pretend, the walkthrough cards behind D149–D151 and D159.</summary>
    public static IEnumerable<CardDefinition> Pretends =>
    [
        new() // Target creature gets +0 Power. Pretend: target creature gets +1 Power.
        {
            Id = "test.mirage-might", Name = "Mirage Might", Type = CardType.Spell, Targets = [new TargetSpec("T1", TargetKind.Creature)],
            Instructions = [new Instruction("power", "T1", 0)], Pretend = new Pretend(Instructions: [new Instruction("power", "T1", 1)]),
        },
        new() // Target creature gets +1 Power. Pretend: target creature gets +1 Power until end of turn.
        {
            Id = "test.mirage-3", Name = "Mirage 3", Type = CardType.Spell, Targets = [new TargetSpec("T1", TargetKind.Creature)],
            Instructions = [new Instruction("power", "T1", 1)], Pretend = new Pretend(Instructions: [new Instruction("power", "T1", 1)], Duration: 1),
        },
        new() // Target creature gets −1 Power. Pretend: no effect.
        {
            Id = "test.curse", Name = "Secret Curse", Type = CardType.Spell, Targets = [new TargetSpec("T1", TargetKind.Creature)],
            Instructions = [new Instruction("power", "T1", -1)], Pretend = new Pretend(Instructions: []),
        },
        new() { Id = "test.goblin", Name = "Goblin", Type = CardType.Creature, Attack = 1, Life = 1, Ap = 4 },
        new() // 5/5 Dragon. Pretend: 1/1 Goblin.
        {
            Id = "test.mimic", Name = "Mimic Dragon", Type = CardType.Creature, Attack = 5, Life = 5, Ap = 4, Pretend = new Pretend("test.goblin"),
        },
        new() { Id = "test.wall", Name = "Wall", Type = CardType.Structure, Life = 5 },
        new() // A creature. Pretend: a Wall (Structure).
        {
            Id = "test.false-wall", Name = "False Wall", Type = CardType.Creature, Attack = 2, Life = 5, Ap = 4, Pretend = new Pretend("test.wall"),
        },
        new()
        {
            Id = "test.wreck", Name = "Wreck", Type = CardType.Spell, Targets = [new TargetSpec("T1", TargetKind.Structure)],
            Instructions = [new Instruction("destroy", "T1")],
        },
    ];

    public static IEnumerable<CardDefinition> All => [Grunt, Mirror, Brute, Mole, Knotter, Digger, DawnSeer(Speed.Reactive), DawnSeer(Speed.Instant), Bastion, Bog, Leaper, Vaulter, Fortify, .. Instructions, .. Pretends];
}
