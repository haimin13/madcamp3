using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataModel : MonoBehaviour
{
    // 싱글톤 인스턴스 설정 (중복 방지)
    public static GameDataModel Instance { get; private set; }

    // 사용자 데이터
    public int userId;
    public string userName;
    public string currentRoomName;
    public string currentRoomPassword;
    public int sessionId = 0;
    public string selectedGame; // gameselectionscene에서 게임 누르면 설정
    public int playerId;
    public string baseUrl;

    void Awake()
    {
        // 싱글톤 패턴 적용: 중복된 GameDataModel 방지
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // 씬 전환 시 파괴되지 않게 함
        sessionId = 0;
        baseUrl = "http://13.60.30.23:5000/Shogi";
    }

    // 필요하다면 Start/Update 사용하세요!
    void Start()
    {
    }
}
