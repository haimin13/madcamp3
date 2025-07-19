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
    public int sessionId;
    public string baseUrl = "";

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
    }

    // 필요하다면 Start/Update 사용하세요!
}
