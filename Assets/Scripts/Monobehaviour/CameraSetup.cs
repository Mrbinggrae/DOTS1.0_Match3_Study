using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField]
    float borderSize;

    void Start()
    {
        SetupCamera();
    }

    private void SetupCamera()
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = em.CreateEntityQuery(typeof(BoardData));
        if (query.CalculateEntityCount() < 1 )
        {
            Invoke(nameof(SetupCamera), 0.05f);
            return;
        }


        var board = query.GetSingleton<BoardData>();

        Camera.main.transform.position = new Vector3((float)(board.BoardSize.x - 1) / 2f, (float)(board.BoardSize.y - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)board.BoardSize.y / 2f + borderSize;
        float horizontalSize = ((float)board.BoardSize.x / 2f + borderSize) / aspectRatio;

        Camera.main.fieldOfView += (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }
}
