using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;

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

[Serializable]
public struct GamePieceData : IComponentData, ICleanupComponentData
{
    public MatchValue MatchValue;
    public int2 Coord;
}
[Serializable]
public struct GamePieceEntity
{
    public Entity Entity;
    public GamePieceData Data;
}

public struct HorizontalMatchedBuffer : IBufferElementData
{
    public Entity MatchEntity;
}
public struct VerticalMatchedBuffer : IBufferElementData
{
    public Entity MatchEntity;
}