using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class MoveRequest
{
    public int session_id;
    public int player_id;
    public string piece;
    public Position position;

    [System.Serializable]
    public class Position
    {
        public PositionDetail from;
        public PositionDetail to; // 이동 전용. -> null일 수도 있음
    }

    [System.Serializable]
    public class PositionDetail
    {
        public int x;
        public int y;
    }
}

public class ShogiController : MonoBehaviour
{
    public ShogiModel model;                 // 인스펙터에서 연결 or Find로 획득
    public List<ShogiPieceInfo> pieceInfos;  // Inspector에 에셋 리스트로 연결
    public ShogiView view;                   // 뷰를 연결 (또는 코드로 할당)
    public int playerId;

    public void OnCellSelected(int x, int y)
    {
        string piece = null;
        if (model.board.cells[x, y].piece != null)
        {
            piece = model.board.cells[x, y].piece.pieceType.ToString();
        }
        var req = new MoveRequest
        {
            session_id = model.session.sessionId,
            player_id = model.GetPlayerId(),
            piece = piece,
            position = {
                from = {
                    x = x,
                    y = y
                },
                to = null
            }
        };
        
    }

    public SessionInfo GetSession()
    {
        SessionInfo session = new SessionInfo();
        session.sessionId = 1;
        session.userId1 = 1;
        session.userId2 = 2;
        return session;
    }

    // Start is called before the first frame update
    void Start()
    {
        // SetPlayerId(userId);
        model.SetPlayerId(0);   // serverless test용
        playerId = model.GetPlayerId();

        model.session = GetSession();

        model.board = new ShogiBoard();
        model.board.InitializeBoard(pieceInfos);

        view.ShowBoard(model.board, playerId);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
