using System;
using Unity.Entities;

[Serializable]
public struct DelayTimerData : IComponentData, ICleanupComponentData
{
    public float Value;
}