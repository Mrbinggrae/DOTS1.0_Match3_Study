using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[BurstCompile]
partial struct GenerateGamePieceEntityHashMapJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public NativeHashMap<int2, Entity> GamePieceEntityMap;

    void Execute(Entity entity, in GamePieceData gamePieceData)
    {
        GamePieceEntityMap[gamePieceData.Coord]=  entity;
    }
}
[BurstCompile]
partial struct GenerateGamePieceDataHashMapJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public NativeHashMap<int2, GamePieceData> GamePieceDataMap;

    void Execute(in GamePieceData gamePieceData)
    {
        GamePieceDataMap[gamePieceData.Coord] = gamePieceData;
    }
}
[BurstCompile]
partial struct GenerateTileEntityHashMapJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public NativeHashMap<int2, Entity> TileHashMap;

    void Execute(Entity entity, in TileData tileData)
    {
        TileHashMap[tileData.Coord] = entity;
    }
}
