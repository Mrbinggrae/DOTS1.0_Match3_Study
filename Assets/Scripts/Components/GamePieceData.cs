using Unity.Entities;
using Unity.Mathematics;

public enum MatchValue
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
    Teal,
    Orange,
    Purple,
}

public struct GamePieceData : IComponentData
{
    public MatchValue MatchValue;
    public int2 Coord;
}