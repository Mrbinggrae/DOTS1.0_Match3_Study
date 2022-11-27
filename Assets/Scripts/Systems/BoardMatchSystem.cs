using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[UpdateAfter(typeof(GamePieceMoveSystem))]
[BurstCompile]
public partial struct BoardMatchSystem : ISystem
{
    EntityQuery m_MatchStateQuery;
    EntityQuery m_MatchCheckGamePieceQuery;
    EntityQuery m_AllGamePieceQeury;

    BufferLookup<HorizontalMatchedBuffer> m_HorizontalMatchedBufferLookup;
    BufferLookup<VerticalMatchedBuffer> m_VerticalMatchedBufferLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_MatchStateQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithNone<DelayTimerData>()
            .WithAll<BoardMatchStateTag>()
            .Build(state.EntityManager);

        m_MatchCheckGamePieceQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GamePieceMatchCheckTag>()
            .WithNone<GamePieceMoveTag>()
            .Build(state.EntityManager);

        m_AllGamePieceQeury = state.GetEntityQuery(ComponentType.ReadOnly<GamePieceData>());

        m_HorizontalMatchedBufferLookup = state.GetBufferLookup<HorizontalMatchedBuffer>();
        m_VerticalMatchedBufferLookup = state.GetBufferLookup<VerticalMatchedBuffer>();
        state.RequireForUpdate(m_MatchStateQuery);

    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var boardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        var boardData = SystemAPI.GetSingleton<BoardData>();
        m_HorizontalMatchedBufferLookup.Update(ref state);
        m_VerticalMatchedBufferLookup.Update(ref state);

        NativeHashMap<int2, GamePieceData> allGamePieceDataMap
            = new(m_AllGamePieceQeury.CalculateEntityCount(), Allocator.TempJob);

        NativeHashMap<int2, Entity> allGamePieceEntityMap
            = new(m_AllGamePieceQeury.CalculateEntityCount(), Allocator.TempJob);

        var dataHashMapJob = new GenerateGamePieceDataHashMapJob
        {
            GamePieceDataMap = allGamePieceDataMap
        }.ScheduleParallel(m_AllGamePieceQeury, state.Dependency);

        var entityHashMapJob = new GenerateGamePieceEntityHashMapJob
        {
            GamePieceEntityMap = allGamePieceEntityMap
        }.ScheduleParallel(m_AllGamePieceQeury, state.Dependency);
        entityHashMapJob.Complete();

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        new GamePieceMatchJob
        {
            ECB = ecb,
            AllGamePieceMap = allGamePieceDataMap,
            BoardSize = boardData.BoardSize,
            AllGamePieceEntityMap = allGamePieceEntityMap,

        }.ScheduleParallel(m_MatchCheckGamePieceQuery, state.Dependency);


        state.EntityManager.RemoveComponent<BoardMatchStateTag>(boardEntity);



    }
}

[BurstCompile]
partial struct GamePieceMatchJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public int2 BoardSize;
    public Entity boardData;

    [ReadOnly]
    public NativeHashMap<int2, GamePieceData> AllGamePieceMap;

    [NativeDisableParallelForRestriction]
    public NativeHashMap<int2, Entity> AllGamePieceEntityMap;

    void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity,ref GamePieceMatchAspect matchAspect)
    {
        matchAspect.FindMatches(ECB, chunkIndex, AllGamePieceMap, AllGamePieceEntityMap, BoardSize);
        ECB.RemoveComponent<GamePieceMatchCheckTag>(chunkIndex, entity);
    }
}