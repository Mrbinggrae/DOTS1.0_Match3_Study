using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;


public readonly partial struct TileAspect : IAspect
{
    readonly RefRW<TileData> Tile;

    public Random Seed
    {
        get => Tile.ValueRO.Randomizer;
        set => Tile.ValueRW.Randomizer = value;
    }

    public int2 Coord
    {
        get => Tile.ValueRO.Coord;
    }

    public GamePieceData GetGamePieceData(GamePiecePrefabBuffer buffer)
    {
        GamePieceData gamePieceData = default;
        gamePieceData.MatchValue = buffer.MatchValue;
        gamePieceData.Coord = Coord;

        Seed = new Random((uint)Seed.NextInt(0, int.MaxValue));
        return gamePieceData;
    }

}