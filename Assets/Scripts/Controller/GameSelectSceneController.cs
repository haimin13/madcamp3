using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.ComponentModel;

public class CreateRoomResponse
{
    public bool result;
    public int session_id;
    public int player_id;
    public string roomName;
    public string roomPW;
}

public class ReadyResponse
{
    public bool startSignal;
}

public class EnterRoomResponse
{
    public bool result;
    public int session_id;
    public int player_id;
    public bool startSignal;
}

public class GameSelectSceneController : MonoBehaviour
{
    public GameDataModel model;
    public GameSelectSceneView view;
    public APIRequester apiRequester;

    private Coroutine readyPollingCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        model = GameDataModel.Instance;
        apiRequester = APIRequester.Instance;
        model.userId = 123;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnGameButtonClicked(string game)
    {
        model.selectedGame = game;
    }

    public void CreateRoom(string roomName)
    {
        if (roomName == null || roomName.Length <= 0)
        {
            view.ShowAlert("You have to enter room name");
            return;
        }
        var req = new Dictionary<string, object>();
        req["user_id"] = model.userId;
        req["game"] = model.selectedGame;
        req["roomName"] = roomName;

        string json = JsonConvert.SerializeObject(req);
        StartCoroutine(apiRequester.PostJson("/create-room", json, (response) =>
        {
            Debug.Log(response);
            var res = JsonConvert.DeserializeObject<CreateRoomResponse>(response);
            if (res.result)
            {
                model.sessionId = res.session_id;
                model.playerId = res.player_id;
                model.currentRoomName = res.roomName;
                model.currentRoomPassword = res.roomPW;
                view.ShowRoomPassword(model.currentRoomPassword);
                if (readyPollingCoroutine != null)
                    StopCoroutine(readyPollingCoroutine);
                readyPollingCoroutine = StartCoroutine(ReadyPollingRoutine());
            }
            else
            {
                Debug.Log("fail to Create Room. Maybe there's a room of same roomName");
            }
        }, (error) =>
        {
            Debug.Log($"no server connection, {error}");
            view.ShowAlert($"no server connection, {error}");
        }
        ));

    }
    public void EnterRoom(string roomName, string password)
    {
        if (roomName == null || roomName.Length <= 0 || password == null || password.Length <= 0)
        {
            view.ShowAlert("You have to enter room name and password");
            return;
        }
        var req = new Dictionary<string, object>();
        req["user_id"] = model.userId;
        req["roomName"] = roomName;
        req["roomPW"] = password;

        string json = JsonConvert.SerializeObject(req);
        StartCoroutine(apiRequester.PostJson("/enter-room", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<EnterRoomResponse>(response);
            if (res.result)
            // if (res.result && res.startSignal)
            {
                model.sessionId = res.session_id;
                model.playerId = res.player_id;
                Debug.Log("Game Start Signal - 씬 이동!");
                SceneManager.LoadScene($"{model.selectedGame}Scene");
            }
            else
            {
                view.ClearRoomNameAndPassword();
            }
        }, (error) =>
        {
            Debug.Log(error);
        }));
    }

    IEnumerator ReadyPollingRoutine()
    {
        while (true)
        {
            // 3초 대기
            // yield return new WaitForSeconds(3f); 즉시 다시 시작
            bool done = false;
            ReadyResponse res = null;

            var req = new Dictionary<string, object>();
            req["session_id"] = model.sessionId;
            req["player_id"] = model.playerId;

            string json = JsonConvert.SerializeObject(req);
            StartCoroutine(apiRequester.PostJson("/ready", json, (response) =>
            {
                res = JsonConvert.DeserializeObject<ReadyResponse>(response);
                done = true;
            }, (error) =>
            {
                Debug.LogWarning($"ReadyPolling 에러: {error}");
                done = true;
            }));

            yield return new WaitUntil(() => done);
            Debug.Log("check done");

            if (res != null)
            {
                if (res.startSignal)
                // if (res.result && res.startSignal)
                {
                    Debug.Log("Game Start Signal - 씬 이동!");
                    readyPollingCoroutine = null; // 필요시 클래스 필드
                    SceneManager.LoadScene($"{model.selectedGame}Scene");
                    yield break;
                }
                else
                    Debug.Log("아직 대기 중...");
                view.ShowAlert("waiting...");
                // 카운트해서 너무 오랫동안 기다리면 룸 파괴?
            }
        }
    }

    public void CloseRoom()
    {
        model.selectedGame = null;
        model.currentRoomName = null;
        model.currentRoomPassword = null;
        model.sessionId = 0;
    }
}
