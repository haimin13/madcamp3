using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="NewShogiPieceInfo", menuName="Shogi/PieceInfo")]
    public class ShogiPieceInfo : ScriptableObject
    {
        public PieceType pieceType;
        public Sprite pieceSprite;

        // 이동 규칙 예시: 상대적 이동 벡터들
        public List<Vector2Int> moveDirections;
    }

public enum PieceType
    { Wang, Chang, Sang, Ja, Hoo }
public class Piece
{
    public PieceType pieceType;
    public int stayedTurns;
    public int owner;
    public ShogiPieceInfo pieceInfo;
}
public class Cell
{
    public int x;
    public int y;
    public Piece piece;
}

public class ShogiBoard
{
    public int width;
    public int height;
    public Cell[,] cells;
    public void InitializeBoard(List<ShogiPieceInfo> pieceInfos)
    {
        width = 3;
        height = 4;
        cells = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new Cell();
                cells[x, y].x = x;
                cells[x, y].y = y;
                cells[x, y].piece = null;
            }
        }
        // Initialize cells
        {
            // Player 0
            cells[0, 0].piece = new Piece
            {
                pieceType = PieceType.Sang,
                stayedTurns = 0,
                owner = 0,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Sang)
            };
            cells[1, 0].piece = new Piece
            {
                pieceType = PieceType.Wang,
                stayedTurns = 0,
                owner = 0,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Wang)
            };
            cells[2, 0].piece = new Piece
            {
                pieceType = PieceType.Chang,
                stayedTurns = 0,
                owner = 0,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Chang)
            };
            cells[1, 1].piece = new Piece
            {
                pieceType = PieceType.Ja,
                stayedTurns = 0,
                owner = 0,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Ja)
            };
            // Player 1
            cells[2, 3].piece = new Piece
            {
                pieceType = PieceType.Sang,
                stayedTurns = 0,
                owner = 1,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Sang)
            };
            cells[1, 3].piece = new Piece
            {
                pieceType = PieceType.Wang,
                stayedTurns = 0,
                owner = 1,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Wang)
            };
            cells[0, 3].piece = new Piece
            {
                pieceType = PieceType.Chang,
                stayedTurns = 0,
                owner = 1,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Chang)
            };
            cells[1, 2].piece = new Piece
            {
                pieceType = PieceType.Ja,
                stayedTurns = 0,
                owner = 1,
                pieceInfo = pieceInfos.Find(p => p.pieceType == PieceType.Ja)
            };
        }
        
    }
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
    public ShogiBoard board;
    public int turn;
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
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
