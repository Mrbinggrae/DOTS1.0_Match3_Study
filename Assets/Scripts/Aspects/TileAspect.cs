using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;


public readonly partial struct TileAspect : IAspect
{
    readonly RefRO<TileData> Tile;

    public int Seed
    {
        get => Tile.ValueRO.Seed;
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