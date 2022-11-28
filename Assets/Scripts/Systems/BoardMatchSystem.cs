using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[UpdateAfter(typeof(GamePieceMoveSystem))]
[BurstCompile]
public partial struct BoardMatchSystem : ISystem
{
    EntityQuery m_BoardMatchStateTagQuery;
    EntityQuery m_MatchCheckGamePieceQuery;
    EntityQuery m_AllGamePieceQeury;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_BoardMatchStateTagQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithNone<DelayTimerData>()
            .WithAll<BoardMatchStateTag>()
            .Build(state.EntityManager);

        m_MatchCheckGamePieceQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GamePieceMatchCheckTag>()
            .WithNone<GamePieceMoveTag>()
            .Build(state.EntityManager);

        m_AllGamePieceQeury = state.GetEntityQuery(ComponentType.ReadOnly<GamePieceData>());

        state.RequireForUpdate(m_BoardMatchStateTagQuery);

    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var boardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        var boardData = SystemAPI.GetSingleton<BoardData>();


        NativeHashMap<int2, GamePieceData> allGamePieceDataMap
            = new(m_AllGamePieceQeury.CalculateEntityCount(), Allocator.TempJob);

        NativeHashMap<int2, Entity> allGamePieceEntityMap
            = new(m_AllGamePieceQeury.CalculateEntityCount(), Allocator.TempJob);

        new GenerateGamePieceDataHashMapJob
        {
            GamePieceDataMap = allGamePieceDataMap
        }.ScheduleParallel(m_AllGamePieceQeury, state.Dependency);

        var entityHashMapJob = new GenerateGamePieceEntityHashMapJob
        {
            GamePieceEntityMap = allGamePieceEntityMap
        }.ScheduleParallel(m_AllGamePieceQeury, state.Dependency);
        entityHashMapJob.Complete();

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecbParallel = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        new GamePieceMatchJob
        {
            ECB = ecbParallel,
            AllGamePieceDataMap = allGamePieceDataMap,
            BoardSize = boardData.BoardSize,
            AllGamePieceEntityMap = allGamePieceEntityMap,

        }.ScheduleParallel(m_MatchCheckGamePieceQuery, state.Dependency).Complete();

        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        ecb.AddComponent(boardEntity, new DelayTimerData { Value = BoardConfig.GAME_DELAY_TIME });
        ecb.AddComponent<BoardCollapseStateTag>(boardEntity);
        ecb.RemoveComponent<BoardMatchStateTag>(boardEntity);

        allGamePieceEntityMap.Dispose();
        allGamePieceDataMap.Dispose();
    }

    [BurstCompile]
    partial struct GamePieceMatchJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public int2 BoardSize;
        public Entity boardData;

        [ReadOnly]
        public NativeHashMap<int2, GamePieceData> AllGamePieceDataMap;

        [NativeDisableParallelForRestriction]
        public NativeHashMap<int2, Entity> AllGamePieceEntityMap;

        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref GamePieceMatchAspect matchAspect)
        {
            matchAspect.FindMatches(ECB, chunkIndex, AllGamePieceDataMap, AllGamePieceEntityMap, BoardSize);
            ECB.RemoveComponent<GamePieceMatchCheckTag>(chunkIndex, entity);
        }
    }
}

