using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct BoardFillSystem : ISystem
{
    EntityQuery m_EmptyTileTagQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_EmptyTileTagQuery = state.GetEntityQuery(ComponentType.ReadWrite<EmptyTileTag>());
        state.RequireForUpdate<BoardFillStateTag>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var boardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        var getPieceAspect = SystemAPI.GetAspectRO<BoardGetRandomGamePieceAspect>(boardEntity);
        if (m_EmptyTileTagQuery.CalculateEntityCount() == 0)
        {
            state.EntityManager.RemoveComponent<BoardFillStateTag>(boardEntity);
            state.EntityManager.AddComponent<BoardInputEnableStateTag>(boardEntity);
            return;
        }

        var beginSimulationSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecbAsParallelWriter = beginSimulationSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var ecb = beginSimulationSystem.CreateCommandBuffer(state.WorldUnmanaged);

        state.Dependency = new GamePieceSpawnJob
        {
            ECB = ecbAsParallelWriter,
            GetPieceAspect = getPieceAspect,
        }.ScheduleParallel(m_EmptyTileTagQuery, state.Dependency);


        ecb.RemoveComponent<BoardFillStateTag>(boardEntity);
        ecb.AddComponent<BoardMoveStateTag>(boardEntity);
    }


    [BurstCompile]
    partial struct GamePieceSpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [ReadOnly]
        public BoardGetRandomGamePieceAspect GetPieceAspect;

        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref TileAspect tileAspect)
        {
            var randomGamePieceBuffer = GetPieceAspect.GetRandomGamePiece(chunkIndex, tileAspect.Seed);
            var gamePieceEntity = ECB.Instantiate(chunkIndex, randomGamePieceBuffer.Prefab);
            var gamePieceData = tileAspect.GetGamePieceData(randomGamePieceBuffer);

            var spawnTransform = UniformScaleTransform.FromPosition(tileAspect.Coord.x, tileAspect.Coord.y + 7f, 0);

            ECB.SetComponent(chunkIndex, gamePieceEntity, new LocalToWorldTransform
            {
                Value = spawnTransform
            });

            ECB.AddComponent(chunkIndex, gamePieceEntity, gamePieceData);
            ECB.AddComponent<GamePieceMoveTag>(chunkIndex, gamePieceEntity);
            ECB.RemoveComponent<EmptyTileTag>(chunkIndex, entity);
        }
    }
}


