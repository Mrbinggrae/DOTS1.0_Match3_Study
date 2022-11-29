using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;

public readonly partial struct GamePieceMatchAspect : IAspect
{
    readonly RefRO<GamePieceData> pieceData;

    int2 Coord
    {
        get => pieceData.ValueRO.Coord;
    }

    private void DestroyMatchedGamePiece
        (EntityCommandBuffer.ParallelWriter ECB,
        int chunkIndex,
        NativeParallelHashMap<int2, GamePieceEntity> allEntity,
        int matchCount,
        int2 matchDirection
        )
    {

        for (int i = 0; i < matchCount; i++)
        {
            int nextX = Coord.x + (int)math.clamp(matchDirection.x, -1, 1) * i;
            int nextY = Coord.y + (int)math.clamp(matchDirection.y, -1, 1) * i;
            ECB.DestroyEntity(chunkIndex, allEntity[new(nextX, nextY)].Entity);
        }
    }

    public void FindMatches(
        EntityCommandBuffer.ParallelWriter ECB, 
        int chunkIndex, 
        NativeParallelHashMap<int2, GamePieceEntity> allEntity,
        int2 boardSize, int minLength = 3)
    {
        int verMatchCount = FindMatchVertical(ECB, chunkIndex, allEntity, boardSize, 3);
        int horMatchCount = FindMatchHorizontal(ECB, chunkIndex, allEntity, boardSize, 3);
    }

    public int FindMatchVertical(EntityCommandBuffer.ParallelWriter ECB,
        int chunkIndex,
        NativeParallelHashMap<int2, GamePieceEntity> allGamePieceEntity,
        int2 boardSize,
        int minLength = 3)
    {

        int downwardMatchCount = FindMatcheCount(allGamePieceEntity, new int2(0, -1), boardSize, 2);
        int upwardMatchCount = FindMatcheCount(allGamePieceEntity, new int2(0, 1), boardSize, 2);

        int matchCount = downwardMatchCount + upwardMatchCount;

        if (downwardMatchCount + upwardMatchCount >= minLength)
        {
            DestroyMatchedGamePiece(ECB, chunkIndex, allGamePieceEntity, downwardMatchCount, new int2(0, -1));
            DestroyMatchedGamePiece(ECB, chunkIndex, allGamePieceEntity, upwardMatchCount, new int2(0, 1));
        }

        return matchCount;
    }

    public int FindMatchHorizontal(EntityCommandBuffer.ParallelWriter ECB,
        int chunkIndex,
        NativeParallelHashMap<int2, GamePieceEntity> allGamePieceEntity,
        int2 boardSize,
        int minLength = 3)
    {
        int leftMatchCount = FindMatcheCount(allGamePieceEntity, new int2(-1, 0), boardSize);
        int rightMatchCount = FindMatcheCount(allGamePieceEntity, new int2(1, 0), boardSize);

        int matchCount = leftMatchCount + rightMatchCount;

        if (leftMatchCount + rightMatchCount >= minLength)
        {
            DestroyMatchedGamePiece(ECB, chunkIndex, allGamePieceEntity, leftMatchCount, new int2(-1, 0));
            DestroyMatchedGamePiece(ECB, chunkIndex, allGamePieceEntity, rightMatchCount, new int2(1, 0));
        }

        return matchCount;
    }

    public int FindMatcheCount(NativeParallelHashMap<int2, GamePieceEntity> allGamePieceEntity, int2 searchDirection, int2 boardSize, int minLength = 2)
    {
        int MatchCount = 1;

        if (!BoardQeury.IsWithinBounds(boardSize, Coord.x, Coord.y)) return 0;


        var maxValue = (boardSize.x > boardSize.y) ? boardSize.x : boardSize.y;


        for (int i = 1; i < maxValue - 1; i++)
        {
            int nextX = Coord.x + (int)math.clamp(searchDirection.x, -1, 1) * i;
            int nextY = Coord.y + (int)math.clamp(searchDirection.y, -1, 1) * i;

            if (!BoardQeury.IsWithinBounds(boardSize, nextX, nextY)) break;
            if (!allGamePieceEntity.ContainsKey(new(nextX, nextY))) break;

            GamePieceData nextPiece = allGamePieceEntity[new int2(nextX, nextY)].Data;


            // 매칭 여부 확인
            if (nextPiece.MatchValue == pieceData.ValueRO.MatchValue)
            {
                MatchCount++;
            }

            else break;
        }

        if (MatchCount >= minLength) return MatchCount;
        else return 0;
    }

}