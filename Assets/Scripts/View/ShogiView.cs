using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShogiView : MonoBehaviour
{
    public GameObject cellPrefab;        // 셀 프리팹 (스프라이트 or 사각형)
    public GameObject piecePrefab;       // 기물 프리팹 (스프라이트렌더러 포함)
    public Transform boardRoot;          // 보드 전체의 부모 오브젝트
    public ShogiModel model;
    public ShogiController controller;
    public Sprite wangSprite, changSprite, sangSprite, jaSprite, hooSprite; // 기물별 스프라이트

    Sprite GetSprite(PieceType type)
    {
        switch(type)
        {
            case PieceType.Wang:  return wangSprite;
            case PieceType.Chang: return changSprite;
            case PieceType.Sang:  return sangSprite;
            case PieceType.Ja:    return jaSprite;
            case PieceType.Hoo:   return hooSprite;
            default:              return null;
        }
    }

    public void ShowBoard()
    {
        int width = model.board.GetLength(0);
        int height = model.board.GetLength(1);
        int playerId = model.GetPlayerId();  // 1 or 2
        float cellSize = 2f;

        Vector3 boardOrigin = new Vector3(-(width - 1) / 2f * cellSize, -(height - 1) / 2f * cellSize, 0);

        // 기존 오브젝트 모두 삭제 (새로 그릴 때)
        foreach (Transform child in boardRoot)
            Destroy(child.gameObject);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // --- 화면 좌표 변환 (player 기준 보드 회전) ---
                int drawX, drawY;
                if (playerId == 1) // 내 시점 그리기
                {
                    drawX = x;
                    drawY = y;
                }
                else // playerId == 2, 180도 회전된 보드
                {
                    drawX = width - 1 - x;
                    drawY = height - 1 - y;
                }
                float gap = 0.1f;
                Vector3 pos = boardOrigin + new Vector3(drawX * (cellSize - gap), drawY * (cellSize - gap), 0);

                var cellObj = Instantiate(cellPrefab, pos, Quaternion.identity, boardRoot);
                var cellView = cellObj.GetComponent<CellView>();
                cellView.Init(x, y, drawX, drawY, this);

                var cellSr = cellObj.GetComponent<SpriteRenderer>();
                if (cellSr != null)
                {
                    if (drawY == 0)
                        cellSr.color = new Color(0f, 1f, 0f, 0.5f); // 초록색 + 50% 투명도
                    else if (drawY == height - 1)
                        cellSr.color = new Color(1f, 0f, 0f, 0.5f); // 빨강색 + 50% 투명도
                    else
                        cellSr.color = new Color(1f, 1f, 1f, 1f); // 기본 흰색, 불투명   // 나머진 흰색(혹은 원래 색)
                }

                // --- Piece 있으면 피스 만들어 띄우기 ---
                Piece piece = model.board[x, y];
                if (piece.pieceType != PieceType.Empty)
                {
                    var pieceObj = Instantiate(piecePrefab, pos, Quaternion.identity, boardRoot);
                    var sr = pieceObj.GetComponent<SpriteRenderer>();
                    sr.sprite = GetSprite(piece.pieceType);

                    // 내 말(아군, 아래) vs 상대 말(위쪽)의 기준 정하기
                    bool isMine = ((playerId == 1 && piece.owner == 1) || (playerId == 2 && piece.owner == 2));
                    sr.flipY = !isMine;
                    sr.flipX = !isMine;
                }
            }
        }
        foreach (Transform child in boardRoot)
        {
            var cell = child.GetComponent<CellView>();
            if (cell != null)
                cell.Highlight(false);
        }
    }

    public void HighlightMovableCells(List<List<int>> moves)
    {
        int width = model.board.GetLength(0);
        int height = model.board.GetLength(1);
        int playerId = model.GetPlayerId();

        Debug.Log(moves[0][0].ToString() + ',' + moves[0][1].ToString());

        // 모든 셀 하이라이트 끄기
        foreach (Transform child in boardRoot)
        {
            var cell = child.GetComponent<CellView>();
            if (cell != null)
                cell.Highlight(false);
        }

        // 이동 가능 좌표 하이라이트 켜기
        foreach (var pos in moves)
        {
            int x = pos[0];
            int y = pos[1];
            // Player 시점 좌표로 전환
            Vector2Int screenPos = ModelToPlayerCoords(playerId, x, y, width, height);

            // 화면에 있는 cell x,y와 일치하는 CellView 찾기
            foreach (Transform child in boardRoot)
            {
                var cell = child.GetComponent<CellView>();
                if (cell != null && cell.x == screenPos.x && cell.y == screenPos.y)
                {
                    cell.Highlight(true);
                }
            }
        }
    }

    public Vector2Int ModelToPlayerCoords(int playerId, int x, int y, int width, int height)
{
    if (playerId == 1)
        return new Vector2Int(x, y); // 그대로
    else // playerId == 2
        return new Vector2Int(width - 1 - x, height - 1 - y); // 180도 반전
}
    public void OnCellClicked(int x, int y)
    {
        Debug.Log("cell clicked and ShogiView acknowledged");
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
