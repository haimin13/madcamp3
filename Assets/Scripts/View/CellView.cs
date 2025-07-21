using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CellView : MonoBehaviour, IPointerClickHandler
{
    public int x, y, drawX, drawY;
    public ShogiView view;
    public Image cellImage;
    private Color originalColor;

    // 셀 초기화 시, 원래 색을 저장
    public void Init(int x, int y, int drawX, int drawY, ShogiView view)
    {
        this.x = x;
        this.y = y;
        this.drawX = drawX;
        this.drawY = drawY;
        this.view = view;

        if (cellImage == null)
            cellImage = GetComponent<Image>();

        if (cellImage != null)
            originalColor = cellImage.color; // 초기 색 저장!
    }

    // 하이라이트 토글
    public void Highlight(bool on)
    {
        if (cellImage == null) return;

        if (on)
            cellImage.color = new Color(0f, 0.5f, 1f, 0.5f); // 원하는 하이라이트 색상
        else
            cellImage.color = originalColor; // 원래 색상으로 복구
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (view != null)
            view.OnCellClicked(x, y);
    }
}