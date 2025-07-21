using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;

public class LoginResponse {
    public bool result;
    public int user_id;
    public string user_name;
}
public class StartSceneManager : MonoBehaviour
{
    public Button loginButton;
    public TMP_InputField userNameInput;
    public APIRequester apiRequester;
    public GameDataModel model;

    // Start is called before the first frame update
    void Start()
    {
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnLoginButtonClicked()
    {
        // 입력 값 가져오기 (정수/문자 변환 주의)
        string userName = userNameInput.text.Trim();
        CreateUser(userName);
    }
    public void CreateUser(string userName)
    {
        var req = new Dictionary<string, object>();
        req["userName"] = userName;

        string json = JsonConvert.SerializeObject(req);
        StartCoroutine(apiRequester.PostJson("/login", json, (response) =>
        {
            var res = JsonConvert.DeserializeObject<LoginResponse>(response);
            if (res.result)
            {
                model.userId = res.user_id;
                model.userName = res.user_name;
                SceneManager.LoadScene("GameSelectScene");
            }
        }, (error) =>
        {
            Debug.Log($"{error}");
        }));
    }
}
