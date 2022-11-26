using Unity.Entities;
using Unity.Mathematics;

public struct TileData : IComponentData
{
    public int Seed;
    public int2 Coord;
}