using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore;

/// <summary>The handle callers pass around: the authoritative state plus the pipeline that's
/// the only thing allowed to mutate it.</summary>
public sealed class Match
{
    public required TrueState State { get; init; }
    public required EventPipeline Pipeline { get; init; }
}
