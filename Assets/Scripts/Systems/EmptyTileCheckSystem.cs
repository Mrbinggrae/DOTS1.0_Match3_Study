using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateAfter(typeof(BoardCollapseSystem))]
public partial struct EmptyTileCheckSystem : ISystem
{
    EntityQuery m_TileQuery;
    EntityQuery m_GamePieceQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_TileQuery = new EntityQueryBuilder(Allocator.Temp) 
            .WithNone<EmptyTileTag>()
            .WithAll<TileData>()
            .Build(state.EntityManager);

        m_GamePieceQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GamePieceData>()
            .WithAll<LocalToWorld>()
            .Build(state.EntityManager);

        state.RequireForUpdate<BoardSearchEmptyTileStateTag>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecbAsParallelWriter = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        NativeHashMap<int2, GamePieceData> gamePieceHashMap
            = new(m_GamePieceQuery.CalculateEntityCount(), Allocator.TempJob);

        new GenerateGamePieceDataHashMapJob
        {
            GamePieceDataMap = gamePieceHashMap,
        }.ScheduleParallel(m_GamePieceQuery, state.Dependency);


        new AddEmptyTileJob
        {
            ECB = ecbAsParallelWriter,
            GamePieceHashMap = gamePieceHashMap,
        }.ScheduleParallel(m_TileQuery, state.Dependency).Complete();

        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        Entity boardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        ecb.RemoveComponent<BoardSearchEmptyTileStateTag>(boardEntity);
        ecb.AddComponent<BoardMoveStateTag>(boardEntity);

        gamePieceHashMap.Dispose();
    }
}


[BurstCompile]
partial struct AddEmptyTileJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    [ReadOnly]
    public NativeHashMap<int2, GamePieceData> GamePieceHashMap;

    void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TileData tileData)
    {
        if (GamePieceHashMap.ContainsKey(tileData.Coord)) return;

        ECB.AddComponent<EmptyTileTag>(chunkIndex, entity);
    }
}

