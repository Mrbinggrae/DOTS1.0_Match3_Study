using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct DelayTimerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DelayTimerData>();
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var deltaTime = SystemAPI.Time.DeltaTime;
        DelayTimerJob delayJob = new()
        {
            ECB = ecb,
            DeltaTime = deltaTime,
        };

        delayJob.ScheduleParallel();
    }
}

partial struct DelayTimerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;

    void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref DelayTimerData delay)
    {
        delay.Value -= DeltaTime;
        if (delay.Value <= 0)
        {
            ECB.RemoveComponent<DelayTimerData>(chunkIndex, entity);
        }
    }
}