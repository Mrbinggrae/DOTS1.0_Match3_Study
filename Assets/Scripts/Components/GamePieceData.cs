using Unity.Entities;


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
    public MatchValue Value;
}