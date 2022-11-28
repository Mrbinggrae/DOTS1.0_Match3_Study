using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct GamePieceMoveAspect : IAspect
{
    readonly RefRW<GamePieceData> GamePieceData;
    readonly TransformAspect Transform;

    int2 Coord { get => GamePieceData.ValueRO.Coord; set => GamePieceData.ValueRW.Coord = value; }

    public void MoveGamePiece(float DeltaTime)
    {
        var Destination = new float3(Coord.x, Coord.y, 0);
        var direction = Destination - Transform.Position;
        Transform.Position += math.normalize(direction) * DeltaTime * BoardConfig.GAME_PIECE_MOVE_SPEED;
    }

    public bool IsArrived()
    {
        var Destination = new float3(Coord.x, Coord.y, 0);
        if (math.distancesq(Destination, Transform.Position) < 0.1f * 0.1f)
        {
            Transform.Position = Destination;
            return true;
        }

        return false;
    }

}