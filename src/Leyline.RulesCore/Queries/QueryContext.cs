using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Queries;

public sealed record QueryContext(string QueryKind, ActorId? Subject);
