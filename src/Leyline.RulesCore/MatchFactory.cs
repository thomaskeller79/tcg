using Leyline.RulesCore.Events;
using Leyline.RulesCore.Rng;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Turns;

namespace Leyline.RulesCore;

public sealed record CreaturePlacement(PlayerId Owner, CardDefinitionId Definition, HexCoord Position, Layer Layer = Layer.Ground);
public sealed record ChampionPlacement(PlayerId Owner, CardDefinitionId Definition, HexCoord Position);

/// <summary>
/// Builds a ready-to-play Match. D20's real Summoning (cast from hand onto bonded terrain)
/// needs a Hand/Library system not in M1's scope — the test board is pre-populated directly
/// via this factory instead. Direct field initialization here (not via the event pipeline)
/// is intentional: this is match *setup*, analogous to the "seed" in the replay formula
/// (seed + config + command log → deterministic state), not gameplay to be replayed.
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
        IReadOnlyList<ChampionPlacement>? champions = null)
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
                CurrentAp = 0, // stays 0 until Slice 4's ChannelActCommand grants it AP
            };
            state.AddActor(champion);
        }

        var pipeline = new EventPipeline();
        pipeline.RegisterStateBasedCheck(new ChampionDeathCheck());
        pipeline.RegisterStateBasedCheck(new ZeroLifeDestructionCheck());
        var match = new Match { State = state, Pipeline = pipeline };
        TurnEngine.BeginMatch(state, pipeline);
        return match;
    }
}
