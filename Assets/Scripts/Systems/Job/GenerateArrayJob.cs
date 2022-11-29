using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[BurstCompile]
partial struct GenerateGamePieceEntityHashMapJob : IJobEntity
{
    public NativeParallelHashMap<int2, GamePieceEntity>.ParallelWriter GamePieceEntityMap;

    void Execute(Entity entity, in GamePieceData gamePieceData)
    {
        GamePieceEntityMap.TryAdd(gamePieceData.Coord, new GamePieceEntity() { Entity = entity, Data = gamePieceData });
    }
}
[BurstCompile]
partial struct GenerateGamePieceDataHashMapJob : IJobEntity
{
    public NativeParallelHashMap<int2, GamePieceData>.ParallelWriter GamePieceDataMap;

    void Execute(in GamePieceData gamePieceData)
    {
        GamePieceDataMap.TryAdd(gamePieceData.Coord, gamePieceData);
    }
}
[BurstCompile]
partial struct GenerateTileEntityHashMapJob : IJobEntity
{
    public NativeParallelHashMap<int2, Entity>.ParallelWriter TileHashMap;

    void Execute(Entity entity, in TileData tileData)
    {
        TileHashMap.TryAdd(tileData.Coord, entity);
    }
}
