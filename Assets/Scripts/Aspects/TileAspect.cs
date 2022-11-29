using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;


public readonly partial struct TileAspect : IAspect
{
    readonly RefRW<TileData> Tile;

    public int Seed
    {
        get => Tile.ValueRO.Seed;
        set => Tile.ValueRW.Seed = value;
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

        return gamePieceData;
    }
}