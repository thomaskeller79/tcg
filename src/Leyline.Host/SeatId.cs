namespace Leyline.Host;

/// <summary>Host-level identity, distinct from PlayerId (RulesCore-level) — a Host binds a
/// seat to a player; RemoteHost (M6) will bind a network connection to a seat the same way.</summary>
public readonly record struct SeatId(int Value);
