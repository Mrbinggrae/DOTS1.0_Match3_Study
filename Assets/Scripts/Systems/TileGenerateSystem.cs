using Unity.Burst;
using Unity.Entities;



[BurstCompile]
[UpdateAfter(typeof(BoardCollapseSystem))]
public partial struct TileGenerateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    { 
        state.RequireForUpdate<BoardData>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var BoardSize = SystemAPI.GetSingleton<BoardData>().BoardSize;

        for (int x = 0; x < BoardSize.x; x++)
        {
            for (int y = 0; y < BoardSize.y; y++)
            {
                var TileEntity = ecb.CreateEntity();

                ecb.AddComponent(TileEntity, new TileData
                {
                    Seed = UnityEngine.Random.Range(0, int.MaxValue),
                    Coord =  new(x, y)
                });


                ecb.AddComponent<EmptyTileTag>(TileEntity);
            }
        }

        var BoardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        ecb.AddComponent(BoardEntity, new BoardFillStateTag());

        state.Enabled = false;
    }
}