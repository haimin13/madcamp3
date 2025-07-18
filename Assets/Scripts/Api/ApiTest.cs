using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ApiTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CallPing());
    }

    IEnumerator CallPing()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:5000/ping");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("서버 응답: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("에러 발생: " + www.error);
        }
    }
}
