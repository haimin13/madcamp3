using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellView : MonoBehaviour
{
    public int x;
    public int y;
    public int drawX;   // 디버깅용
    public int drawY;   // 디버깅용
    public ShogiView view;
    public SpriteRenderer highlightRenderer; // 미리 분리된 오브젝트 등에 할당

    public void Init(int x, int y, int drawX, int drawY, ShogiView view)
    {
        this.x = x;
        this.y = y;
        this.drawX = drawX;
        this.drawY = drawY;
        this.view = view;
    }

    public void Highlight(bool on)
    {
        if (highlightRenderer != null)
            highlightRenderer.enabled = on; // 예: 컬러 등 효과
    }
    void OnMouseUpAsButton()
    {
        Debug.Log("cell clicked!");
        // 클릭 시 항상 모델 좌표로 전달!
        view.OnCellClicked(x, y);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
