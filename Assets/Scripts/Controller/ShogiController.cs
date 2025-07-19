using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

public class AvailableMovesResponse
{
    public bool result;
    public List<List<int>> moves;
}
public class Capture
{
    public bool is_capture;
    public string piece; // 만약 piece의 타입을 알면 object 대신 해당 타입 사용
}
public class MoveResponse
{
    public bool result;
    public Capture capture;
    public bool is_game_end;
}
public class ShogiController : MonoBehaviour
{
    public ShogiModel model;                 // 인스펙터에서 연결 or Find로 획득  // Inspector에 에셋 리스트로 연결
    public ShogiView view;                   // 뷰를 연결 (또는 코드로 할당)
    public void OnCellSelected(int x, int y)
    {
        // 이동가능 리스트가 있는 경우
        if (model.movablePositions != null && model.movablePositions.Count > 0)
        {
            Debug.Log($"cell clicked and controller acknowledged! x: {x}, y: {y}");
            // 클릭 좌표가 이동가능 좌표에 포함되어 있으면 이동 요청
            bool isMovable = model.movablePositions.Any(pos => pos[0] == x && pos[1] == y);

            if (isMovable)
            {
                if (model.selectedCapturedPiece == null)
                    RequestMoveTo(x, y); // 이동 요청(서버로 보내기 등)
                else
                    RequestDropTo(x, y);
                return;
            }
            else
            {
                model.selectedCapturedPiece = null;
                model.selectedPosition = null;
                model.movablePositions = null;
            }
            view.RemoveHighlights();
        
        }
        // 새로운 셀 클릭
        if (model.board[x, y].pieceType != PieceType.Empty)
        {
            model.selectedPosition = new List<int> { x, y };
            model.selectedCapturedPiece = null;
            RequestAvailableMoves(x, y);
        }
    }

    public void RequestAvailableMoves(int x, int y)
    {
        var req = new Dictionary<string, object>();
        req["session_id"] = model.GetSessionId();
        req["player_id"] = model.GetPlayerId();
        req["piece"] = model.board[x, y].pieceType.ToString();
        req["position"] = new Dictionary<string, object>
        {
            {"from", new List<int> {x, y}},
            {"to", null}
        };

        string json = JsonConvert.SerializeObject(req);

        if (APIRequester.Instance != null)
        {
            StartCoroutine(APIRequester.Instance.PostJson("/shogi/available-moves", json, (response) =>
            {
                var res = JsonConvert.DeserializeObject<AvailableMovesResponse>(response);
                if (res.result)
                {
                    model.movablePositions = res.moves;
                    view.HighlightMovableCells(res.moves);
                }
            }, (response) =>
            {   // JUST FOR TEST
                var moves = new List<List<int>>
                    {
                        new() {1,1},
                        new() {2,2},
                        new() {0,0}
                    };
                model.movablePositions = moves;
                view.HighlightMovableCells(moves);
            }));
        }
    }

    public void RequestMoveTo(int x, int y)
    {
        int fromX = model.selectedPosition[0];
        int fromY = model.selectedPosition[1];

        var req = new Dictionary<string, object>();
        req["session_id"] = model.GetSessionId();
        req["player_id"] = model.GetPlayerId();
        req["piece"] = model.board[fromX, fromY].pieceType.ToString();
        req["position"] = new Dictionary<string, object>
        {
            {"from", new List<int> {fromX, fromY}},
            {"to", new List<int> {x, y}}
        };

        string json = JsonConvert.SerializeObject(req);

        if (APIRequester.Instance != null)
        {
            StartCoroutine(APIRequester.Instance.PostJson("/shogi/move", json, (response) =>
            {
                var res = JsonConvert.DeserializeObject<MoveResponse>(response);
                if (res.result)
                {
                    if (res.is_game_end)
                    {
                        //TODO
                    }
                    else if (res.capture.is_capture)
                    {
                        var pieceType = (PieceType)System.Enum.Parse(typeof(PieceType), res.capture.piece);
                        var piece = new Piece
                        {
                            pieceType = pieceType,
                            stayedTurns = 0,
                            owner = model.GetPlayerId()
                        };
                        model.playersInfo[model.GetPlayerId()].capturedPieces.Add(piece);
                    }
                    model.board[x, y] = model.board[fromX, fromY];
                    model.board[fromX, fromY] = model.CreateEmptyPiece();

                    // 다 끝나고
                    model.selectedPosition = null;
                    model.movablePositions = null;
                    model.selectedCapturedPiece = null;
                }
            }));
        }
    }
    public void RequestDropTo(int x, int y)
    {
        var req = new Dictionary<string, object>();
        req["session_id"] = model.GetSessionId();
        req["player_id"] = model.GetPlayerId();
        req["piece"] = model.selectedCapturedPiece.pieceType.ToString();
        req["position"] = new Dictionary<string, object>
        {
            {"from", null},
            {"to", new List<int> {x, y}}
        };
        string json = JsonConvert.SerializeObject(req);

        if (APIRequester.Instance != null)
        {
            StartCoroutine(APIRequester.Instance.PostJson("/shogi/drop", json, (response) =>
            {
                var res = JsonConvert.DeserializeObject<MoveResponse>(response);
                if (res.result)
                {
                    model.board[x, y] = model.selectedCapturedPiece;
                    // 다 끝나고
                    model.selectedPosition = null;
                    model.movablePositions = null;
                    model.selectedCapturedPiece = null;
                }
            }));
        }

    }
    public void OnCapturedPieceClicked(Piece piece)
    {
        if (model.selectedCapturedPiece != null && model.selectedCapturedPiece == piece)
        {
            model.selectedCapturedPiece = null;
            model.movablePositions = null;
            return;
        }
        else
        {
            model.selectedCapturedPiece = piece;
            RequestAvailableDrop();
        }
    }

    public void RequestAvailableDrop()
    {
        var req = new Dictionary<string, object>();
        req["session_id"] = model.GetSessionId();
        req["player_id"] = model.GetPlayerId();
        req["piece"] = model.selectedCapturedPiece.pieceType.ToString();
        req["position"] = new Dictionary<string, object>
        {
            {"from", null},
            {"to", null}
        };

        string json = JsonConvert.SerializeObject(req);

        if (APIRequester.Instance != null)
        {
            StartCoroutine(APIRequester.Instance.PostJson("/shogi/available-drop", json, (response) =>
            {
                var res = JsonConvert.DeserializeObject<AvailableMovesResponse>(response);
                if (res.result)
                {
                    model.movablePositions = res.moves;
                    view.HighlightMovableCells(res.moves);
                }
            }, (response) =>
            {   // JUST FOR TEST
                var moves = new List<List<int>>
                    {
                        new() {0,1},
                        new() {2,1},
                        new() {0,2},
                        new() {2,2}
                    };
                model.movablePositions = moves;
                view.HighlightMovableCells(moves);
            }));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // model.SetPlayerId(userId);
        model.SetPlayerId(1);   // serverless test용

        model.SetSessionId(GameDataModel.Instance.sessionId);

        model.InitializePlayers();
        model.InitializeBoard();

        view.ShowBoard();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
