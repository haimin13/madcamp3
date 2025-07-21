using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class StartSceneView : MonoBehaviour
{
    public Button loginButton;
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
        int userId = 123;
        string userName = "hamin";

        // GameDataModel에 저장
        GameDataModel.Instance.userId = userId;
        GameDataModel.Instance.userName = userName;

        // 씬 전환 (GameSelectScene으로)
        SceneManager.LoadScene("GameSelectScene");
    }
}
