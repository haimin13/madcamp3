using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSelectSceneView : MonoBehaviour
{
    public GameSelectSceneController controller;
    public GameDataModel model;

    public Button shogiButton;

    public GameObject lobbyPanel;
    public Button createRoomButton;
    public Button enterRoomButton;
    public Button closeButtonLobby;

    public GameObject createRoomPanel;
    public TMP_InputField createRoomNameInput;
    public Button createRoomConfirmButton;
    public TMP_Text createRoomPasswordPlaceholder;

    public GameObject enterRoomPanel;
    public TMP_InputField enterRoomNameInput;
    public TMP_InputField enterRoomPasswordInput;
    public Button enterRoomConfirmButton;

    public List<Button> closeButtonCEs;
    public TMP_Text alertText;

    // Start is called before the first frame update
    void Start()
    {
        // 버튼 리스너 등록
        shogiButton.onClick.AddListener(OnShogiButtonClicked);
        createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        enterRoomButton.onClick.AddListener(OnEnterRoomButtonClicked);
        createRoomConfirmButton.onClick.AddListener(OnCreateRoomConfirmClicked);
        enterRoomConfirmButton.onClick.AddListener(OnEnterRoomConfirmClicked);
        closeButtonLobby.onClick.AddListener(OnCloseLobbyClicked);
        foreach (var btn in closeButtonCEs)
            btn.onClick.AddListener(OnCloseCEClicked);

        lobbyPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        enterRoomPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnShogiButtonClicked()
    {
        controller.OnGameButtonClicked("Shogi");
        lobbyPanel.SetActive(true);
    }

    public void OnCreateRoomButtonClicked()
    {
        createRoomPanel.SetActive(true);
        createRoomNameInput.text = "";
        createRoomPasswordPlaceholder.text = "it will be auto-generated";
    }

    public void OnEnterRoomButtonClicked()
    {
        enterRoomPanel.SetActive(true);
        enterRoomNameInput.text = "";
        enterRoomPasswordInput.text = "";
    }

    public void OnCreateRoomConfirmClicked()
    {
        if (model == null)
        {
            Debug.Log("model is null");
        }
        if (model.sessionId != 0)
        {
            ShowAlert("Room already created and waiting");
            return;
        }
        string roomName = createRoomNameInput.text.Trim();
        controller.CreateRoom(roomName);
    }

    public void OnEnterRoomConfirmClicked()
    {
        string roomName = enterRoomNameInput.text.Trim();
        var pw = enterRoomPasswordInput.text.Trim();
        controller.EnterRoom(roomName, pw);
    }

    public void ShowRoomPassword(string roomPassword)
    {
        createRoomPasswordPlaceholder.text = roomPassword;
    }

    public void ClearRoomNameAndPassword()
    {
        enterRoomNameInput.text = "";
        enterRoomPasswordInput.text = "";
        ShowAlert("Wrong room name or password. try again.");
    }

    public void ShowAlert(string message)
    {
        if (alertText == null) return;
        alertText.text = message;
        alertText.gameObject.SetActive(true);

        // 기존 코루틴이 있다면 중복 방지
        StopCoroutine("HideAlertCoroutine");
        StartCoroutine(HideAlertCoroutine());
    }

    IEnumerator HideAlertCoroutine()
    {
        yield return new WaitForSeconds(3f);
        if (alertText != null)
            alertText.gameObject.SetActive(false);
    }

    public void OnCloseLobbyClicked()
    {
        controller.CloseLobby();
        lobbyPanel.SetActive(false);
    }
    public void OnCloseCEClicked()
    {
        Debug.Log("ceclose");
        controller.CloseRoom();
        createRoomPanel.SetActive(false);
        enterRoomPanel.SetActive(false);
    }
}
