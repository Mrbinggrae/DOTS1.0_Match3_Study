using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class GamePieceAuthoring : MonoBehaviour
{
    public GameObject presentationGamePiecePrefab;

    class Baker : Baker<GamePieceAuthoring>
	{
		public override void Bake(GamePieceAuthoring authoring)
		{
            AddComponent<GamePieceData>();
            AddBuffer<HorizontalMatchedBuffer>();
            AddBuffer<VerticalMatchedBuffer>();


            PresentationGO pgo = new()
            {
                Prefab = authoring.presentationGamePiecePrefab
            };
            AddComponentObject(pgo);
		}
	}
}

public class PresentationGO : IComponentData
{
	public GameObject Prefab;
}

public class TransformGO : IComponentData, ICleanupComponentData
{
    public Transform Transform;
}



public partial struct PresentationGOSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        var BeginSimulationBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecbBOS = BeginSimulationBuffer.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (pgo, entity) in SystemAPI.Query<PresentationGO>().WithEntityAccess())
        {
            GameObject go = GameObject.Instantiate(pgo.Prefab);
            go.transform.position = new Vector3(100, 100, 100);
            ecbBOS.AddComponent(entity, new TransformGO { Transform = go.transform });
            ecbBOS.RemoveComponent<PresentationGO>(entity);
        }

        var ecbEOS = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (goTransform, transform) in SystemAPI.Query<TransformGO, TransformAspect>())
        {
            goTransform.Transform.SetPositionAndRotation(transform.Position, transform.Rotation);
        }

        foreach (var (goTransform, entity) in SystemAPI.Query<TransformGO>().WithNone<LocalToWorld>().WithEntityAccess())
        {
            GameObject.Destroy(goTransform.Transform.gameObject);
            ecbEOS.RemoveComponent<TransformGO>(entity);
        }
    }
}