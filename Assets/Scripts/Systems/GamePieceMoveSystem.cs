using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.EventSystems.EventTrigger;

[BurstCompile]
[UpdateAfter(typeof(BoardFillSystem))]
public partial struct GamePieceMoveSystem : ISystem
{
    EntityQuery m_MoveTagQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_MoveTagQuery = state.GetEntityQuery(ComponentType.ReadWrite<GamePieceMoveTag>());
        state.RequireForUpdate<BoardMoveStateTag>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var boardEntity = SystemAPI.GetSingletonEntity<BoardData>();


       var beginSimECBSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var beginECB = beginSimECBSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var deltaTime = SystemAPI.Time.DeltaTime;

        new GamePieceMoveJob
        {
            ECB = beginECB,
            DeltaTime = deltaTime

        }.ScheduleParallel(m_MoveTagQuery, state.Dependency);

        if(m_MoveTagQuery.CalculateEntityCount() == 0)
        {
            state.EntityManager.RemoveComponent<BoardMoveStateTag>(boardEntity);
            state.EntityManager.AddComponent<BoardMatchStateTag>(boardEntity);
            state.EntityManager.AddComponentData(boardEntity, new DelayTimerData
            {
                Value = BoardConfig.GAME_DELAY_TIME
            });
        }
    }
}

[BurstCompile]
partial struct GamePieceMoveJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;

    void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref GamePieceMoveAspect moveAspect)
    {
        if (moveAspect.IsArrived())
        {
            ECB.RemoveComponent<GamePieceMoveTag>(chunkIndex, entity);

            ECB.AddComponent<GamePieceMatchCheckTag>(chunkIndex, entity);
        }
        else
        moveAspect.MoveGamePiece(DeltaTime);

    }
}