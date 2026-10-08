using System.Numerics;

namespace Leyline.RulesCore.Model;

/// <summary>A set of Elements, read as a disjunction ("any one of these").</summary>
[Flags]
public enum ElementSet : byte
{
    None = 0,
}

public static class ElementSets
{
    public static ElementSet Of(params Element[] elements) =>
        elements.Aggregate(ElementSet.None, (set, e) => set | (ElementSet)(1 << (int)e));

    public static IEnumerable<Element> Elements(this ElementSet set) =>
        Enum.GetValues<Element>().Where(e => (set & Of(e)) != 0);

    public static int Width(this ElementSet set) => BitOperations.PopCount((uint)set);

    public static string Name(this ElementSet set) => string.Join("/", set.Elements());
}

/// <summary>D125: a mana pip of a cost — generic (no Elements: any mana pays it) or colored, a
/// disjunction over Elements (one Element = ordinary colored mana, two = hybrid, …).</summary>
public readonly record struct ManaPip(ElementSet Colors)
{
    public static readonly ManaPip Generic = new(ElementSet.None);

    public static ManaPip Of(params Element[] elements) => new(ElementSets.Of(elements));

    public bool IsGeneric => Colors == ElementSet.None;

    public override string ToString() => IsGeneric ? "{1}" : $"{{{Colors.Name()}}}";
}

/// <summary>D125: one mana in a pool — colorless (no Elements) or colored, a disjunction over
/// Elements: it can be spent as any one of them.</summary>
public readonly record struct ManaUnit(ElementSet Colors)
{
    public static readonly ManaUnit Colorless = new(ElementSet.None);

    public static ManaUnit Of(params Element[] elements) => new(ElementSets.Of(elements));

    /// <summary>"Colorless", or Element names joined by '/', e.g. "Fire/Metal".</summary>
    public static ManaUnit Parse(string text) =>
        text.Equals("Colorless", StringComparison.OrdinalIgnoreCase)
            ? Colorless
            : Of(text.Split('/').Select(name => Enum.Parse<Element>(name.Trim(), ignoreCase: true)).ToArray());

    /// <summary>A generic pip takes any mana; a colored pip takes mana sharing one of its Elements.</summary>
    public bool Pays(ManaPip pip) => pip.IsGeneric || (Colors & pip.Colors) != ElementSet.None;

    /// <summary>This dominates <paramref name="other"/> if it pays every pip the other pays —
    /// for these two kinds of pip, exactly when its Elements include the other's.</summary>
    public bool Dominates(ManaUnit other) => (Colors & other.Colors) == other.Colors;

    public override string ToString() => Colors == ElementSet.None ? "Colorless" : Colors.Name();
}

/// <summary>
/// D125: the semi-automatic payment. Pips are paid from the most constrained to the least — one
/// Element, then two, …, generic last. Each pip takes the mana type every other type able to pay
/// it dominates; when several types tie, the player chooses among them (a type dominating
/// another candidate is never offered). The payment is not searched ahead.
/// </summary>
public static class ManaPayment
{
    /// <summary>Every payment the rule leaves open, each as the mana spent; empty if the pool
    /// can't pay. Payments that spend the same mana are listed once.</summary>
    public static IReadOnlyList<IReadOnlyList<ManaUnit>> Options(IReadOnlyDictionary<ManaUnit, int> pool, IReadOnlyList<ManaPip> pips)
    {
        var order = pips.Select((p, i) => (p, i))
            .OrderBy(x => x.p.IsGeneric ? int.MaxValue : x.p.Colors.Width()).ThenBy(x => x.i)
            .Select(x => x.p).ToList();
        var left = pool.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
        var results = new List<IReadOnlyList<ManaUnit>>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var spent = new List<ManaUnit>();

        void Recurse(int index)
        {
            if (index == order.Count)
            {
                var sorted = spent.OrderBy(u => (int)u.Colors).ToList();
                if (seen.Add(string.Join(",", sorted.Select(u => (int)u.Colors))))
                    results.Add(sorted);
                return;
            }
            var candidates = left.Where(kv => kv.Value > 0 && kv.Key.Pays(order[index])).Select(kv => kv.Key).ToList();
            var minimal = candidates.Where(c => !candidates.Any(d => d != c && c.Dominates(d))).OrderBy(c => (int)c.Colors);
            foreach (var unit in minimal)
            {
                left[unit]--;
                spent.Add(unit);
                Recurse(index + 1);
                spent.RemoveAt(spent.Count - 1);
                left[unit]++;
            }
        }

        Recurse(0);
        return results;
    }
}
