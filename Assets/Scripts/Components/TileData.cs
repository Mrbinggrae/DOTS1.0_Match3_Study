using Unity.Entities;
using Unity.Mathematics;

public struct TileData : IComponentData
{
    public Random Randomizer;
    public int2 Coord;
}