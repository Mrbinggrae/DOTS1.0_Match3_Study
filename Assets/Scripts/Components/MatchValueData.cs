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

public struct MatchValueData : IComponentData
{
    public MatchValue Value;
}