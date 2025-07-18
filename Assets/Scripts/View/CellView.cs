using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellView : MonoBehaviour
{
    public int x;
    public int y;
    public ShogiView view;

    void OnMouseUpAsButton()
    {
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
