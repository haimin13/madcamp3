using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    Wang, Chang, Sang, Ja, Hoo, Empty
}
public class Piece
{
    public PieceType pieceType;
    public int stayedTurns;
    public int owner;
}

public class ShogiPlayer
{
    public int userId;
    public string userName;
    public int playerId;
    public List<Piece> capturedPieces;
}

public class SessionInfo
{
    public int sessionId;
    public int userId1;
    public int userId2;
}
public class ShogiModel : MonoBehaviour
{
    public Piece[,] board;
    public int turn;    // 지금 누구 턴인가? 1 or 2
    private int playerId;
    public SessionInfo session;

    public void SetPlayerId(int value)
    {
        playerId = value;
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    public void InitializeBoard()
    {
        int width = 3;
        int height = 4;
        board = new Piece[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                board[x, y] = new Piece
                {
                    pieceType = PieceType.Empty,
                    owner = 0,
                    stayedTurns = 0
                };
            }
        }
        {
            board[0, 0].pieceType = PieceType.Sang;
            board[0, 0].owner = 1;
            board[1, 0].pieceType = PieceType.Wang;
            board[1, 0].owner = 1;
            board[2, 0].pieceType = PieceType.Chang;
            board[2, 0].owner = 1;
            board[1, 1].pieceType = PieceType.Ja;
            board[1, 1].owner = 1;

            board[2, 3].pieceType = PieceType.Sang;
            board[2, 3].owner = 2;
            board[1, 3].pieceType = PieceType.Wang;
            board[1, 3].owner = 2;
            board[0, 3].pieceType = PieceType.Chang;
            board[0, 3].owner = 2;
            board[1, 2].pieceType = PieceType.Ja;
            board[1, 2].owner = 2;
        }

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
