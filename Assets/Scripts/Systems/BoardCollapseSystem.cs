using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Tilemaps;

[UpdateAfter(typeof(BoardMatchSystem))]
[BurstCompile]
public partial struct BoardCollapseSystem : ISystem
{
    EntityQuery m_DestroyGamePieceQuery;
    EntityQuery m_GamePieceQuery;
    EntityQuery m_CollapseStateTagQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_DestroyGamePieceQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithNone<LocalToWorld>()
            .WithAll<GamePieceData>()
            .Build(state.EntityManager);

        m_GamePieceQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LocalToWorld>()
            .WithAll<GamePieceData>()
            .Build(state.EntityManager);

        m_CollapseStateTagQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<BoardCollapseStateTag>()
            .WithNone<DelayTimerData>()
            .Build(state.EntityManager);

        state.RequireForUpdate(m_CollapseStateTagQuery);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var boardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        if (m_DestroyGamePieceQuery.CalculateEntityCount() == 0)
        {
            state.EntityManager.RemoveComponent<BoardCollapseStateTag>(boardEntity);
            state.EntityManager.AddComponent<BoardFillStateTag>(boardEntity);
            return;
        }

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecbParallelWriter = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        NativeParallelHashMap<int2, GamePieceData> destroyGamePieceMap
           = new(m_DestroyGamePieceQuery.CalculateEntityCount(), Allocator.TempJob);

        
        var GenerateMapJobHandle = new GenerateGamePieceDataHashMapJob
        {
            GamePieceDataMap = destroyGamePieceMap.AsParallelWriter(),
        }.ScheduleParallel(m_DestroyGamePieceQuery, state.Dependency);

        state.Dependency = new CollapseGamePieceJob
        {
            ECB = ecbParallelWriter,
            DestroyGamePieceDataMap = destroyGamePieceMap,

        }.ScheduleParallel(m_GamePieceQuery, GenerateMapJobHandle);

        state.Dependency.Complete();


        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        ecb.RemoveComponent<GamePieceData>(m_DestroyGamePieceQuery.ToEntityArray(Allocator.Temp));



        ecb.RemoveComponent<BoardCollapseStateTag>(boardEntity);
        ecb.AddComponent<BoardSearchEmptyTileStateTag>(boardEntity);

        destroyGamePieceMap.Dispose();

    }

    [BurstCompile]
    partial struct CollapseGamePieceJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [ReadOnly]
        public NativeParallelHashMap<int2, GamePieceData> DestroyGamePieceDataMap;

        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref GamePieceData gamePieceData)
        {
            int2 GamePieceCoord = gamePieceData.Coord;

            int FallCount = 0;
            for (int y = GamePieceCoord.y - 1; y >= 0; y--)
            {
                int2 FallCoord = new(GamePieceCoord.x, y);

                if (!DestroyGamePieceDataMap.ContainsKey(FallCoord)) continue;

                FallCount++;
            }

            gamePieceData.Coord.y -= FallCount;

            if (FallCount > 0) ECB.AddComponent<GamePieceMoveTag>(chunkIndex, entity);
        }
    }
}