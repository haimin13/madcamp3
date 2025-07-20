using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ShogiView : MonoBehaviour
{
    public GameObject cellPrefab;        // 셀 프리팹 (스프라이트 or 사각형)
    public GameObject piecePrefab;       // 기물 프리팹 (스프라이트렌더러 포함)
    public Transform boardRoot;          // 보드 전체의 부모 오브젝트
    public ShogiModel model;
    public ShogiController controller;
    public RectTransform myCapturedPanel;
    public RectTransform opponentCapturedPanel;
    public GameObject capturedPieceIconPrefab;
    public GameObject myTurnPanel;    // 내 턴일 때 표시할 패널
    public GameObject opTurnPanel;    // 상대 턴일 때 표시할 패널
    public TextMeshProUGUI timeText;

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

    public void UpdateTimer(int secondsLeft)
    {
        timeText.text = secondsLeft.ToString(); // 시간 텍스트 표시
    }
    public void DisplayTurn()
    {
        myTurnPanel.SetActive(model.myTurn);      // 내 턴이면 내 턴 패널 활성화
        opTurnPanel.SetActive(!model.myTurn);     // 내 턴이 아니면 상대 턴 패널 활성화
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
        RemoveHighlights();
        SetupCapturedPanels();
        ShowCapturedPieces();
    }
    public void RemoveHighlights()
    {
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

        if (moves == null)
        {
            Debug.Log("null passed in moves");
        }
        else if (moves.Count == 0)
        {
            Debug.Log("moves is empty");
        }
        else
        {
            Debug.Log(moves[0][0].ToString() + ',' + moves[0][1].ToString());
        }

        // 모든 셀 하이라이트 끄기
        RemoveHighlights();

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
    
    void ShowCapturedPieces()
    {
        int playerId = model.GetPlayerId();
        int adversaryId = playerId == 1 ? 2 : 1;

        // 패널 초기화: 자식 모두 삭제
        foreach (Transform t in myCapturedPanel) Destroy(t.gameObject);
        foreach (Transform t in opponentCapturedPanel) Destroy(t.gameObject);

        // 그리드 설정
        int MAX_COUNT = 6;
        int COLS = 3;   // 가로 칸수
        int ROWS = 2;   // 세로 칸수

        void DrawPieces(List<Piece> capturedPieces, RectTransform panel)
        {
            int count = Mathf.Min(MAX_COUNT, capturedPieces.Count);
            Vector2 panelSize = panel.rect.size;

            float cellWidth = panelSize.x / COLS;
            float cellHeight = panelSize.y / ROWS;

            for (int i = 0; i < count; i++)
            {
                Piece piece = capturedPieces[i];
                if (piece == null) continue;

                int col = i % COLS;
                int row = i / COLS;

                var obj = Instantiate(capturedPieceIconPrefab, panel);
                var rt = obj.GetComponent<RectTransform>();

                // 왼쪽 위(0,0) 기준이므로 y는 반전!
                float x = col * cellWidth + cellWidth / 2f;
                float y = -row * cellHeight - cellHeight / 2f + panelSize.y / 2f;
                rt.anchoredPosition = new Vector2(x, y);

                var img = obj.GetComponent<Image>();
                Sprite pieceSprite = GetSprite(piece.pieceType);
                if (img != null) img.sprite = pieceSprite;

                var btn = obj.GetComponent<Button>();
                if (btn == null)
                    btn = obj.AddComponent<Button>();

                var capturedPiece = piece; // 클로저 이슈 방지
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    OnCapturedPieceClicked(capturedPiece);
                });
            }
        }

        DrawPieces(model.playersInfo[playerId].capturedPieces, myCapturedPanel);
        DrawPieces(model.playersInfo[adversaryId].capturedPieces, opponentCapturedPanel);
    }


    void SetupCapturedPanels()
    {
        int playerId = model.GetPlayerId();

        if(playerId == 1)
        {
            // 내 패널 = 오른쪽 아래, 상대 패널 = 왼쪽 위로 이동
            myCapturedPanel.anchorMin = new Vector2(1, 0); myCapturedPanel.anchorMax = new Vector2(1, 0);
            opponentCapturedPanel.anchorMin = new Vector2(0, 1); opponentCapturedPanel.anchorMax = new Vector2(0, 1);
        }
        else
        {
            // 내 패널 = 오른쪽 아래, 상대 패널 = 왼쪽 위 (2P도 동일, 논리적으로 필요시 패널 위치 스왑 가능)
            myCapturedPanel.anchorMin = new Vector2(1, 0); myCapturedPanel.anchorMax = new Vector2(1, 0);
            opponentCapturedPanel.anchorMin = new Vector2(0, 1); opponentCapturedPanel.anchorMax = new Vector2(0, 1);
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

    public void OnCapturedPieceClicked(Piece piece)
    {
        RemoveHighlights();
        // Controller로 이벤트 위임
        controller.OnCapturedPieceClicked(piece);
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
