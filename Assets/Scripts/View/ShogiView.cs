using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShogiView : MonoBehaviour
{
    public GameObject cellPrefab;     // 슬롯/칸 프리팹 (인스펙터에 등록)
    public GameObject piecePrefab;    // 기물 프리팹 (인스펙터에 등록)
    public Transform boardRoot;       // 보드의 parent 오브젝트

    public ShogiController controller;

    public void ShowBoard(ShogiBoard board, int playerId)
    {
        float cellSize = 1.0f;
        Vector3 boardOrigin = new Vector3(-board.width / 2f * cellSize, -board.height / 2f * cellSize, 0);
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                // 뷰 상의 표시 위치만 player에 따라 flip
                int drawX = playerId == 0 ? x : (board.width - 1 - x);
                int drawY = playerId == 0 ? y : (board.height - 1 - y);

                Vector3 pos = boardOrigin + new Vector3(drawX * cellSize, drawY * cellSize, 0);
                var cellGO = Instantiate(cellPrefab, pos, Quaternion.identity, boardRoot);

                // CellView에 실제 모델 좌표 할당
                var cellView = cellGO.GetComponent<CellView>();
                cellView.x = x;
                cellView.y = y;
                cellView.view = this;

                // 기물 표시 등도 같은 방식
                var piece = board.cells[x, y].piece;
                if (piece != null)
                {
                    var pieceGO = Instantiate(piecePrefab, pos, Quaternion.identity, boardRoot);
                    var sr = pieceGO.GetComponent<SpriteRenderer>();
                    sr.sprite = piece.pieceInfo.pieceSprite;
                    if (piece.owner == 1)
                    {
                        sr.flipX = true;
                        sr.flipY = true;
                    }
                }
            }
        }
    }
    public void OnCellClicked(int x, int y)
    {
        // x, y는 모델(Board) 기준!
        // 바로 Controller/Model로 true 좌표 전달
        controller.OnCellSelected(x, y);
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
