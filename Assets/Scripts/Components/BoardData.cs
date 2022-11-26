using Unity.Entities;
using System;
using Unity.Mathematics;
using Unity.Collections;


[Serializable]
public struct BoardData : IComponentData
{
}


public struct BoardConfig
{
    public static readonly float GAME_PIECE_MOVE_SPEED = 10f;
    public static readonly float GAME_DELAY_TIME = 0.3f;

    public static int2 BoardSize { get; private set; }
    public static void SetBoardSize(int2 value)
    {
       BoardSize = value;
    }
}

public struct BoardQeury
{
    public static bool IsWithinBounds(int2 boardSize, int x, int y)
    {
        return (x >= 0 && x < boardSize.x && y >= 0 && y < boardSize.y);
    }

    public static bool IsWithinBounds(int2 boardSize, int2 coord)
    {
        return (coord.x >= 0 && coord.x < boardSize.x && coord.y >= 0 && coord.y < boardSize.y);
    }

}