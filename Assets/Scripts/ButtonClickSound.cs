using UnityEngine;
using UnityEngine.UI;
public class ButtonClickSound : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(OnClickButton);
    }

    void OnClickButton()
    {
        audioSource.PlayOneShot(clickSound);
        DoButtonFunction();
    }

    void DoButtonFunction()
    {
        Debug.Log("��ư ��� �����!");
        // ���� ��ư ��� ���⼭ ����
    }
}
