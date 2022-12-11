using System.Linq;
using Unity.Entities;
using Unity.VisualScripting;
using Random = Unity.Mathematics.Random;


public readonly partial struct BoardGetRandomGamePieceAspect : IAspect
{
    readonly DynamicBuffer<GamePiecePrefabBuffer> PiecePrefabBuffers;

    public GamePiecePrefabBuffer GetRandomGamePiece(int chunkIndex, Random random)
    {
        var PieceBuffer = PiecePrefabBuffers[random.NextInt(0, PiecePrefabBuffers.Length)];
        return PieceBuffer;
    }
}