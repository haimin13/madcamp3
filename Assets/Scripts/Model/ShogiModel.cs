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
    public int stayedTurns; // maybe bool?
    public int owner;
}

public class ShogiPlayer
{
    public int userId;
    public string userName;
    public List<Piece> capturedPieces;
}

public class ShogiModel : MonoBehaviour
{
    public Piece[,] board;
    public int turn;    // 지금 누구 턴인가? 1 or 2
    private int playerId;
    private int adversaryId;
    public Dictionary<int, ShogiPlayer> playersInfo = new Dictionary<int, ShogiPlayer>();
    private int sessionId;
    public bool myTurn;
    public bool isWin = false;
    public int timeLimit = 90;
    public int timeLeft;
    public List<int> selectedPosition;
    public Piece selectedCapturedPiece;
    public List<List<int>> movablePositions = null;


    public void SetPlayerId(int value)
    {
        playerId = value;
    }

    public int GetPlayerId()
    {
        return playerId;
    }
    public void SetAdversaryId(int value)
    {
        adversaryId = value;
    }
    public int GetAdversaryId()
    {
        return adversaryId;
    }
    public void InitializeBoard()
    {
        int width = 3;
        int height = 4;
        board = new Piece[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                board[x, y] = CreateEmptyPiece();

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
    public int GetSessionId() {
        return sessionId;
    }
    public void SetSessionId(int num) {
        sessionId = num;
    }
    public void InitializePlayers()
    {
        for (int i = 1; i < 3; i++)
        {
            playersInfo[i] = new ShogiPlayer
            {
                userId = i,
                userName = $"player{i}",
                capturedPieces = new List<Piece> { }
            };
            //for TEST
            /*{
                playersInfo[i].capturedPieces.Add(new Piece
                {
                    pieceType = PieceType.Chang,
                    owner = i,
                    stayedTurns = 0
                });
                playersInfo[i].capturedPieces.Add(new Piece
                {
                    pieceType = PieceType.Chang,
                    owner = i,
                    stayedTurns = 0
                });
                playersInfo[i].capturedPieces.Add(new Piece
                {
                    pieceType = PieceType.Chang,
                    owner = i,
                    stayedTurns = 0
                });
                playersInfo[i].capturedPieces.Add(new Piece
                {
                    pieceType = PieceType.Chang,
                    owner = i,
                    stayedTurns = 0
                });
                playersInfo[i].capturedPieces.Add(new Piece
                {
                    pieceType = PieceType.Chang,
                    owner = i,
                    stayedTurns = 0
                });
                playersInfo[i].capturedPieces.Add(new Piece
                {
                    pieceType = PieceType.Chang,
                    owner = i,
                    stayedTurns = 0
                });

            }
            */
        }

    }

    public Piece CreateEmptyPiece()
    {
        var piece = new Piece
        {
            pieceType = PieceType.Empty,
            owner = 0,
            stayedTurns = 0
        };
        return piece;
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
