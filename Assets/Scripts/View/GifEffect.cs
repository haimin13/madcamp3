using UnityEngine;
using UnityEngine.UI;

public class GifEffect : MonoBehaviour
{
    private Sprite[] frames;     // GIF 프레임으로 사용할 스프라이트 배열
    [SerializeField] private float frameDelay = 0.05f; // 프레임 간 딜레이 (초)
    private int index = 0;
    private float timer = 0;
    public ShogiView view;

    private SpriteRenderer spriteRenderer;
    private Image uiImage;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) uiImage = GetComponent<Image>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        timer += Time.deltaTime;
        if (timer >= frameDelay)
        {
            index = (index + 1) % frames.Length;
            if (spriteRenderer != null) spriteRenderer.sprite = frames[index];
            else if (uiImage != null) uiImage.sprite = frames[index];
            timer = 0;
        }
    }
    
    public void LoadFrames(string resourcePath)
    {
        frames = Resources.LoadAll<Sprite>(resourcePath);

        if (frames == null || frames.Length == 0)
            Debug.LogWarning($"GifEffect: '{resourcePath}' 경로에 스프라이트가 없습니다.");

        index = 0;
        timer = 0;

        // 첫 프레임 즉시 적용 (필요 시)
        if(spriteRenderer != null && frames.Length > 0) spriteRenderer.sprite = frames[0];
        else if(uiImage != null && frames.Length > 0) uiImage.sprite = frames[0];
    }
}
