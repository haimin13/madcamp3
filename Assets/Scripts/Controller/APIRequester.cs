using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class APIRequester : MonoBehaviour
{
    public static APIRequester Instance { get; private set; }
    public string baseUrl;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator PostJson(string api, string json, System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        baseUrl = GameDataModel.Instance.baseUrl;
        var url = baseUrl + api;
        Debug.Log(url);
        Debug.Log(json);
        
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = 30;
        yield return request.SendWebRequest();

        Debug.Log("ResponseBody: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.Log("POST Failed: " + request.error);
            onError?.Invoke(request.error);
        }
    }
}
