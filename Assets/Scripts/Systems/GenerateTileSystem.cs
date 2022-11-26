using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;



public partial struct GenerateTileSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoardData>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var BoardSize = BoardConfig.BoardSize;

        for (int x = 0; x < BoardSize.x; x++)
        {
            for (int y = 0; y < BoardSize.y; y++)
            {
                var TileEntity = ecb.CreateEntity();

                ecb.AddComponent(TileEntity, new TileData {
                    Seed = UnityEngine.Random.Range(0, int.MaxValue)
                });

                ecb.AddComponent(TileEntity, new CoordinateData
                {
                    Coord = new(x, y)
                });

                ecb.AddComponent<EmptyTileTag>(TileEntity);
            }
        }

        var BoardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        ecb.AddComponent(BoardEntity, new BoardFillStateTag());

        state.Enabled = false;
    }
}