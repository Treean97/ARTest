using System;

public readonly struct PairKey : IEquatable<PairKey>
{
    public readonly int Min;
    public readonly int Max;

    public PairKey(int id1, int id2)
    {
        if (id1 <= id2) { Min = id1; Max = id2; }
        else           { Min = id2; Max = id1; }
    }

    public bool Equals(PairKey other) => Min == other.Min && Max == other.Max;
    public override bool Equals(object obj) => obj is PairKey other && Equals(other);

    public override int GetHashCode()
    {
        unchecked { return (Min * 397) ^ Max; }
    }

    public override string ToString() => $"({Min},{Max})";
}