using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(BoardFillSystem))]
[BurstCompile]
public partial struct BoardMatchSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoardMatchStateTag>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("BoardFillSystem");
        var boardEntity = SystemAPI.GetSingletonEntity<BoardData>();
        state.EntityManager.RemoveComponent<BoardMatchStateTag>(boardEntity);
    }
}