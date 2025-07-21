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
    public bool is_end;
    public int winner;
}
public class Move
{
    public List<int> from;
    public List<int> to;
}
public class WaitResponse
{
    public bool result;
    public bool turn;
    public Move op_position;
    public string op_piece;
    public bool is_end;
    public int winner;
}
public class TimeOutResponse
{
    public bool result;
    public bool is_end;
    public int winner;
}
public class ShogiController : MonoBehaviour
{
    public ShogiModel model;                 // 인스펙터에서 연결 or Find로 획득  // Inspector에 에셋 리스트로 연결
    public ShogiView view;                   // 뷰를 연결 (또는 코드로 할당)
    private Coroutine turnPollingCoroutine;
    private Coroutine timerCoroutine;
    public APIRequester apiRequester;
    public void OnCellSelected(int x, int y)
    {
        Debug.Log($"cell clicked and controller acknowledged! x: {x}, y: {y}");
        if (!model.myTurn)
        {
            view.ShowAlert("Not your turn!");
            return;
        }
        // 이동가능 리스트가 있는 경우
        if (model.movablePositions != null && model.movablePositions.Count > 0)
        {
            Debug.Log("model.movablePositions Not NULL");
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
                Debug.Log("Not movable spot");
                model.selectedCapturedPiece = null;
                model.selectedPosition = null;
                model.movablePositions = null;
                view.RemoveHighlights();
            }
        }
        // 새로운 셀 클릭
        if (model.board[x, y].pieceType != PieceType.Empty && model.board[x, y].owner == model.GetPlayerId())
        {
            model.selectedPosition = new List<int> { x, y };
            model.selectedCapturedPiece = null;
            RequestAvailableMoves(x, y);
        }
    }

    public void RequestAvailableMoves(int x, int y)
    {
        Debug.Log("RequestAvailableMove");
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

        
        StartCoroutine(apiRequester.PostJson("/available-moves", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<AvailableMovesResponse>(response);
            if (res.result)
            {
                model.movablePositions = res.moves;
                view.HighlightMovableCells(res.moves);
            }
        }, (error) =>
        {   // JUST FOR TEST
            Debug.Log(error);
            var moves = new List<List<int>>
                {
                    new() {1,1},
                    new() {2,2},
                    new() {0,0},
                    new() {0,3}
                };
            model.movablePositions = moves;
            view.HighlightMovableCells(moves);
        }));
    }

    public void RequestMoveTo(int x, int y)
    {
        int fromX = model.selectedPosition[0];
        int fromY = model.selectedPosition[1];

        if (model.board[fromX, fromY].pieceType == PieceType.Ja &&
        ((model.GetPlayerId() == 1 && y == 3) || (model.GetPlayerId() == 2 && y == 0)))
            model.board[fromX, fromY].pieceType = PieceType.Hoo;

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


        StartCoroutine(apiRequester.PostJson("/move", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<MoveResponse>(response);
            if (res.result)
            {
                Piece sourcePiece = model.board[fromX, fromY];
                Piece movedPiece = new Piece
                {
                    pieceType = sourcePiece.pieceType,
                    owner = sourcePiece.owner,
                    stayedTurns = 0
                };
                model.board[x, y] = movedPiece;
                model.board[fromX, fromY] = model.CreateEmptyPiece();

                if (res.capture != null && res.capture.is_capture)
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
                DebugBoard();

                model.selectedPosition = null;
                model.movablePositions = null;
                model.selectedCapturedPiece = null;
                view.RemoveHighlights();
                view.ShowBoard();

                if (res.is_end)
                {
                    GameOver(res.winner);
                }
                else
                    ChangeTurn(false); // 턴 넘기고 폴링
            }
        }, (error) => {Debug.Log(error); }));
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
        StartCoroutine(apiRequester.PostJson("/drop", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<MoveResponse>(response);
            if (res.result)
            {
                Piece droppedPiece = model.selectedCapturedPiece;
                Piece newPiece = new Piece
                {
                    pieceType = droppedPiece.pieceType,
                    owner = droppedPiece.owner,
                    stayedTurns = 0
                };
                model.board[x, y] = newPiece;
                model.playersInfo[model.GetPlayerId()].capturedPieces.Remove(droppedPiece);
                DebugBoard();

                // 다 끝나고
                model.selectedPosition = null;
                model.movablePositions = null;
                model.selectedCapturedPiece = null;
                view.RemoveHighlights();
                view.ShowBoard();

                if (res.is_end)
                {
                    GameOver(res.winner);
                }
                else
                    ChangeTurn(false);
            }
        },(error) => {Debug.Log(error);}));
    }
    public void OnCapturedPieceClicked(Piece piece)
    {
        if (!model.myTurn)
        {
            view.ShowAlert("Not your turn!");
            return;
        }
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
        StartCoroutine(apiRequester.PostJson("/available-drop", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<AvailableMovesResponse>(response);
            if (res.result)
            {
                model.movablePositions = res.moves;
                view.HighlightMovableCells(res.moves);
            }
        }, (error) =>
        {
            Debug.Log(error);
            // JUST FOR TEST
            // var moves = new List<List<int>>
            //     {
            //         new() {0,1},
            //         new() {2,1},
            //         new() {0,2},
            //         new() {2,2}
            //     };
            // model.movablePositions = moves;
            // view.HighlightMovableCells(moves);
        }));
    }
    // 폴링 루프: 3초마다 내 턴인지 확인 (내 턴이 오면 코루틴 종료, 입력 허용)
    IEnumerator TurnPollingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            bool done = false;
            WaitResponse res = null;

            // 서버에 /wait API 요청
            string json = JsonConvert.SerializeObject(new {
                session_id = model.GetSessionId(),
                player_id = model.GetPlayerId()
            });

            StartCoroutine(apiRequester.PostJson("/wait-turn", json, (response) => {
                res = JsonConvert.DeserializeObject<WaitResponse>(response);
                done = true;
            }, (error) => { done = true; }));

            yield return new WaitUntil(() => done);
            Debug.Log("check done");

            if (res != null && res.result)
            {
                if (res.turn)
                {
                    if (res.op_position != null && res.op_position.to != null)
                    {
                        Debug.Log("상대 말에 변화있음!");
                        int toX = res.op_position.to[0];
                        int toY = res.op_position.to[1];

                        if (res.op_position.from != null) // 상대 말 이동
                        {
                            Debug.Log("상대 말 이동함!");
                            int fromX = res.op_position.from[0];
                            int fromY = res.op_position.from[1];
                            Piece toPiece = model.board[toX, toY];

                            if (toPiece.pieceType != PieceType.Empty && toPiece.owner == model.GetPlayerId())
                            {
                                Piece capturePiece = new Piece
                                {
                                    pieceType = (toPiece.pieceType == PieceType.Hoo) ? PieceType.Ja : toPiece.pieceType,
                                    owner = model.GetAdversaryId(),
                                    stayedTurns = 0
                                };

                                model.playersInfo[model.GetAdversaryId()].capturedPieces.Add(capturePiece);
                            }

                            Piece sourcePiece = model.board[fromX, fromY];
                            if (sourcePiece.pieceType == PieceType.Ja && (
                                (model.GetAdversaryId() == 1 && toY == 3) || (model.GetAdversaryId() == 2 && toY == 0)))
                                sourcePiece.pieceType = PieceType.Hoo;

                            Piece movedPiece = new Piece
                            {
                                pieceType = sourcePiece.pieceType,
                                owner = sourcePiece.owner,
                                stayedTurns = 0
                            };
                            
                            model.board[toX, toY] = movedPiece;
                            model.board[fromX, fromY] = model.CreateEmptyPiece();
                            DebugBoard();
                            view.ShowBoard();
                        }
                        else // 상대 말 드롭
                        {
                            Debug.Log("상대 말 드랍함!");
                            PieceType droppedType = (PieceType)System.Enum.Parse(typeof(PieceType), res.op_piece);

                            Piece newPiece = new Piece
                            {
                                pieceType = droppedType,
                                owner = model.GetAdversaryId(),
                                stayedTurns = 0
                            };
                            model.board[toX, toY] = newPiece;
                            var droppedPiece = model.playersInfo[model.GetAdversaryId()].capturedPieces
                                .Where(piece => piece.pieceType == droppedType)
                                .FirstOrDefault();

                            model.playersInfo[model.GetAdversaryId()].capturedPieces.Remove(droppedPiece);
                            DebugBoard();
                            view.ShowBoard();
                        }
                    }
                    else Debug.Log("상대 말에 변화안함!");
                    if (res.is_end)
                    {
                        GameOver(res.winner);
                        yield break;
                    }
                    else
                    {
                        ChangeTurn(true);
                        yield break;
                    }
                }
            }
        }
    }

    // 타이머 코루틴: 1초마다 timeLeft 감소, 시간초과 처리
    IEnumerator TimerRoutine()
    {
        model.timeLeft = model.timeLimit;
        view.UpdateTimer(model.timeLeft); // UI에 남은 시간 표시(옵션)
        while (model.timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            model.timeLeft--;
            view.UpdateTimer(model.timeLeft); // UI update(옵션)
        }
        // 내 턴일 때 타임오버 처리
        if (model.myTurn)
            OnTimerExpired();
    }

    public void ChangeTurn(bool myTurn)
    {
        string json = JsonConvert.SerializeObject(model.board);
        Debug.Log(json);
        model.myTurn = myTurn;
        view.DisplayTurn();
        StartTimer();
        if (!model.myTurn)
        {
            if (turnPollingCoroutine == null)
                turnPollingCoroutine = StartCoroutine(TurnPollingRoutine());
        }
        else
        {
            turnPollingCoroutine = null;
        }
    }

    public void StartTimer()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    void OnTimerExpired()
    {
        Debug.Log("시간초과!");
        string json = JsonConvert.SerializeObject(new
        {
            session_id = model.GetSessionId(),
            player_id = model.GetPlayerId()
        });
        // 서버에 알림, 게임 종료 등 추가 처리
        StartCoroutine(apiRequester.PostJson("/timeout", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<TimeOutResponse>(response);
            if (res.result)
                if (res.is_end)
                    GameOver(res.winner);
        }, (error)=>
        {
            Debug.Log(error);
        }));
    }

    private void GetSessionData() {
        model.SetPlayerId(GameDataModel.Instance.playerId);
        model.SetAdversaryId((GameDataModel.Instance.playerId == 1) ? 2 : 1);
        model.SetSessionId(GameDataModel.Instance.sessionId);
        model.myTurn = model.GetPlayerId() == 1;
    }

    public void GameOver(int winner)
    {
        view.ShowBoard();
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = null;

        model.isWin = winner == model.GetPlayerId();
        view.ShowGameOver(model.isWin);
    }

    public void DebugBoard()
    {
        Debug.Log($"{model.board[0, 3].pieceType.ToString()}{model.board[1, 3].pieceType.ToString()}{model.board[2, 3].pieceType.ToString()}");
        Debug.Log($"{model.board[0, 2].pieceType.ToString()}{model.board[1, 2].pieceType.ToString()}{model.board[2, 2].pieceType.ToString()}");
        Debug.Log($"{model.board[0, 1].pieceType.ToString()}{model.board[1, 1].pieceType.ToString()}{model.board[2, 1].pieceType.ToString()}");
        Debug.Log($"{model.board[0, 0].pieceType.ToString()}{model.board[1, 0].pieceType.ToString()}{model.board[2, 0].pieceType.ToString()}");
        
    }

    // Start is called before the first frame update
    void Start()
    {
        GetSessionData();

        model.InitializePlayers();
        model.InitializeBoard();

        view.ShowBoard();
        view.DisplayTurn();
        model.timeLeft = model.timeLimit;
        StartTimer();

        if (!model.myTurn)
        {
            if (turnPollingCoroutine == null)
                turnPollingCoroutine = StartCoroutine(TurnPollingRoutine());
        }
    }

    // Update is called once per frame
    void Update() { }
}
