using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using System;


public class BoardAuthoring : MonoBehaviour
{
    [SerializeField]
    int2 boardSize;


    [SerializeField]
    MatchValuePrefabDictionary matchValuePrefabDictionary;

    class Baker : Baker<BoardAuthoring>
	{
		public override void Bake(BoardAuthoring authoring)
		{
            BoardConfig.SetBoardSize(authoring.boardSize);

            AddComponent<BoardData>();

            DynamicBuffer<GamePiecePrefabBuffer> piecePrefabBuffers = AddBuffer<GamePiecePrefabBuffer>();

            foreach (KeyValuePair<MatchValue, GameObject> piece in authoring.matchValuePrefabDictionary)
            {
                GamePiecePrefabBuffer buffer = default;
                buffer.Prefab = GetEntity(piece.Value);
                buffer.MatchValue = piece.Key;
                piecePrefabBuffers.Add(buffer);
            }
        }
	}
}


[InternalBufferCapacity(8)]
[Serializable]
public struct GamePiecePrefabBuffer : IBufferElementData
{
    public Entity Prefab;
    public MatchValue MatchValue;
}
