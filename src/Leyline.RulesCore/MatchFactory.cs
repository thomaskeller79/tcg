using Leyline.RulesCore.Events;
using Leyline.RulesCore.Rng;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Turns;

namespace Leyline.RulesCore;

public sealed record CreaturePlacement(PlayerId Owner, CardDefinitionId Definition, HexCoord Position, Layer Layer = Layer.Ground);
public sealed record ChampionPlacement(PlayerId Owner, CardDefinitionId Definition, HexCoord Position);

/// <summary>Initial Library/Hand contents for one player — Library[0] is the top (next draw).
/// Either list may be empty/omitted; a player with no Champion.Draw ability or an empty
/// Library simply never draws.</summary>
public sealed record PlayerSetup(PlayerId Player, IReadOnlyList<CardDefinitionId> Library, IReadOnlyList<CardDefinitionId> Hand);

/// <summary>A pre-bonded terrain node at setup — applied before the first Beginning phase
/// runs, so mana is already live on turn 1 instead of requiring a real Bond + a full turn
/// cycle first. Same "setup, not gameplay" footing as initial placement (D8's bonding is
/// otherwise a runtime-only mutation, TerrainPipeline.Bond).</summary>
public sealed record TerrainBond(PlayerId Player, HexCoord Coord);

/// <summary>
/// Builds a ready-to-play Match. Direct field initialization here (not via the event
/// pipeline) is intentional: this is match *setup*, analogous to the "seed" in the replay
/// formula (seed + config + command log → deterministic state), not gameplay to be replayed —
/// this now includes each player's starting Library/Hand (D20's Draw/Cast pipelines are real,
/// but *dealing* a deck is still setup, same footing as initial board placement).
/// </summary>
public static class MatchFactory
{
    public static Match CreateMatch(
        Board board,
        IReadOnlyList<PlayerId> playerIds,
        IReadOnlyList<CreaturePlacement> creatures,
        MatchConfig config,
        ICardDefinitionRepository content,
        ulong seed,
        IReadOnlyList<ChampionPlacement>? champions = null,
        IReadOnlyList<PlayerSetup>? playerSetups = null,
        IReadOnlyList<TerrainBond>? bonds = null)
    {
        var state = new TrueState
        {
            Board = board,
            Players = playerIds.Select(id => new PlayerState { Id = id }).ToList(),
            PhaseSequence = StandardPhases.Sequence,
            Config = config,
            Content = content,
            Rng = RngState.FromSeed(seed),
            ActivePlayer = playerIds[0],
        };

        foreach (var setup in playerSetups ?? [])
        {
            var player = state.Players.First(p => p.Id == setup.Player);
            player.Library.AddRange(setup.Library);
            player.Hand.AddRange(setup.Hand);
        }

        foreach (var placement in creatures)
        {
            var def = content.Get(placement.Definition);
            var actor = new CreatureState
            {
                Id = state.AllocateActorId(),
                Owner = placement.Owner,
                Definition = placement.Definition,
                Position = placement.Position,
                Layer = placement.Layer,
                Located = placement.Layer != Layer.Below, // D12: the below layer is hidden by default
                Life = def.Life,
                CurrentAp = def.MaxAp,
            };
            state.AddActor(actor);
        }

        foreach (var placement in champions ?? [])
        {
            var def = content.Get(placement.Definition);
            var champion = new ChampionState
            {
                Id = state.AllocateActorId(),
                Owner = placement.Owner,
                Definition = placement.Definition,
                Position = placement.Position,
                Life = def.Life,
                CurrentAp = 0, // refreshed to MaxAp by the first Beginning phase's RefreshApEffect, like any actor
            };
            state.AddActor(champion);
        }

        foreach (var bond in bonds ?? [])
        {
            var champion = state.AllActors.OfType<ChampionState>().First(c => c.Owner == bond.Player);
            champion.Network.Bond(bond.Coord);
        }

        var pipeline = new EventPipeline();
        pipeline.RegisterStateBasedCheck(new ChampionDeathCheck());
        pipeline.RegisterStateBasedCheck(new ZeroLifeDestructionCheck());
        var match = new Match { State = state, Pipeline = pipeline };
        TurnEngine.BeginMatch(state, pipeline);
        return match;
    }
}
