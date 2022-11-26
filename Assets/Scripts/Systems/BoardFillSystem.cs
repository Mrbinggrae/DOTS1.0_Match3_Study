using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public partial struct BoardFillSystem : ISystem
{
    EntityQuery EmptyTileTagQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EmptyTileTagQuery = state.GetEntityQuery(ComponentType.ReadWrite<EmptyTileTag>());

        state.RequireForUpdate<BoardFillStateTag>();
        state.RequireForUpdate(EmptyTileTagQuery);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var beginSimulationSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecbAsParallelWriter = beginSimulationSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var ecb = beginSimulationSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var boardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        var getPieceAspect = SystemAPI.GetAspectRO<BoardGetRandomGamePieceAspect>(boardEntity);

        var spawnJobHandle = new GamePieceSpawnJob
        {
            ECB = ecbAsParallelWriter,
            GetPieceAspect = getPieceAspect
        }.ScheduleParallel(EmptyTileTagQuery, state.Dependency);
        spawnJobHandle.Complete();

        state.Dependency = spawnJobHandle;

        ecb.RemoveComponent<BoardFillStateTag>(boardEntity);
    }
}

[BurstCompile]
partial struct GamePieceSpawnJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    [ReadOnly]
    public BoardGetRandomGamePieceAspect GetPieceAspect;

    void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in TileAspect tileAspect)
    {
        var randomGamePieceBuffer = GetPieceAspect.GetRandomGamePiece(chunkIndex, tileAspect.Seed);
        var gamePieceEntity = ECB.Instantiate(chunkIndex, randomGamePieceBuffer.Prefab);
        var gamePieceData = tileAspect.GetGamePieceData(randomGamePieceBuffer);


        ECB.AddComponent(chunkIndex, gamePieceEntity, gamePieceData);
        ECB.RemoveComponent<EmptyTileTag>(chunkIndex, entity);
    }
}