using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

public class AvailableMovesResponse
{
    public bool result;
    public List<List<int>> moves;
}
public class ShogiController : MonoBehaviour
{
    public ShogiModel model;                 // 인스펙터에서 연결 or Find로 획득  // Inspector에 에셋 리스트로 연결
    public ShogiView view;                   // 뷰를 연결 (또는 코드로 할당)
    public int playerId;

    public void OnCellSelected(int x, int y)
    {
        Debug.Log($"cell clicked and controller acknowledged! x: {x}, y: {y}");
        var req = new Dictionary<string, object>();
        req["session_id"] = model.session.sessionId;
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
                    view.HighlightMovableCells(res.moves);
                }
            }));
        }
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
        // model.SetPlayerId(userId);
        model.SetPlayerId(2);   // serverless test용
        playerId = model.GetPlayerId();

        model.session = GetSession();
        model.InitializeBoard();

        view.ShowBoard();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
