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
    public bool result;
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

    // Start is called before the first frame update
    void Start()
    {
        model = GameDataModel.Instance;
        apiRequester = APIRequester.Instance;
        CloseLobby();
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
            if (res.result && res.session_id != 0)
            {
                model.sessionId = res.session_id;
                model.playerId = res.player_id;
                model.currentRoomName = res.roomName;
                model.currentRoomPassword = res.roomPW;
                view.ShowRoomPassword(model.currentRoomPassword);
                ReadyPolling();
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
            if (res.result && res.session_id != 0)
            // if (res.result && res.startSignal)
            {
                model.sessionId = res.session_id;
                model.playerId = res.player_id;
                Debug.Log("Game Start Signal - 씬 이동!");
                SceneManager.LoadScene($"{model.selectedGame}Scene");
            }
            else
            {
                Debug.Log("no such room exist");
                view.ClearRoomNameAndPassword();
            }
        }, (error) =>
        {
            Debug.Log(error);
        }));
    }

    public void ReadyPolling()
    {
        var req = new Dictionary<string, object>();
        req["session_id"] = model.sessionId;
        req["player_id"] = model.playerId;

        string json = JsonConvert.SerializeObject(req);
        Debug.Log("PollingStart" + json);
        StartCoroutine(apiRequester.PostJson("/ready", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<ReadyResponse>(response);
            if (res.result)
            {
                if (res.startSignal)
                {
                    Debug.Log("User Found!");
                    SceneManager.LoadScene($"{model.selectedGame}Scene");
                }
                else
                {
                    Debug.Log("No User Found");
                    view.ShowAlert("No user found. closing the room");
                    CloseRoom();
                }
            }
        }, (error) =>
        {
            Debug.LogWarning($"ReadyPolling 에러: {error}");
            CloseRoom();
        }));
    }

    public void CloseRoom()
    {
        model.currentRoomName = null;
        model.currentRoomPassword = null;
        model.sessionId = 0;
        model.playerId = 0;
    }
    public void CloseLobby()
    {
        CloseRoom();
        model.selectedGame = null;
    }
}
