using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject gameOverPanel;
    public TextMeshProUGUI winText;
    public Button gameOverButton;
    public TextMeshProUGUI alertText;

    public Sprite wangMy, changMy, sangMy, jaMy, hooMy, wangTheir, changTheir, sangTheir, jaTheir, hooTheir; // 기물별 스프라이트

    Sprite GetSprite(PieceType type, bool isMine)
    {
        switch(type)
        {
            case PieceType.Wang:  return isMine ? wangMy : wangTheir;
            case PieceType.Chang: return isMine ? changMy : changTheir;
            case PieceType.Sang:  return isMine ? sangMy : sangTheir;
            case PieceType.Ja:    return isMine ? jaMy : jaTheir;
            case PieceType.Hoo:   return isMine ? hooMy : hooTheir;
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
        int playerId = model.GetPlayerId();
        float cellSize = 140f; // 픽셀 단위, 패널 크기에 맞게 설정!

        Vector2 boardOrigin = new Vector2(-(width - 1) / 2f * cellSize, -(height - 1) / 2f * cellSize);

        // 기존 자식 모두 삭제
        foreach (Transform child in boardRoot)
            Destroy(child.gameObject);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 시점 변환
                int drawX, drawY;
                if (playerId == 1) { drawX = x; drawY = y; }
                else { drawX = width - 1 - x; drawY = height - 1 - y; }

                Vector2 pos = boardOrigin + new Vector2(drawX * cellSize, drawY * cellSize);

                // 셀 생성 (Image 기반)
                var cellObj = Instantiate(cellPrefab, boardRoot);
                var rt = cellObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = pos;

                var cellImg = cellObj.GetComponent<Image>();
                // 배경색 지정 (초록/빨강/흰)
                if (drawY == 0)
                    cellImg.color = new Color(0f, 1f, 0f, 0.5f);
                else if (drawY == height - 1)
                    cellImg.color = new Color(1f, 0f, 0f, 0.5f);
                else
                    cellImg.color = Color.white;

                var cellView = cellObj.GetComponent<CellView>();
                if (cellView != null)
                    cellView.Init(x, y, drawX, drawY, this);

                // Piece 있으면
                Piece piece = model.board[x, y];
                if (piece.pieceType != PieceType.Empty)
                {
                    var pieceObj = Instantiate(piecePrefab, cellObj.transform); // parent 변경!
                    var pieceRt = pieceObj.GetComponent<RectTransform>();
                    var pieceSize = cellSize * 0.8f;
                    pieceRt.sizeDelta = new Vector2(pieceSize, pieceSize);
                    pieceRt.anchoredPosition = Vector2.zero; // 셀 내부 중앙 배치 (local)

                    var img = pieceObj.GetComponent<Image>();
                    img.sprite = GetSprite(piece.pieceType, piece.owner == model.GetPlayerId());

                    // 회전/플립 필요시
                    if (piece.owner != model.GetPlayerId())
                        img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
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
            //Vector2Int screenPos = ModelToPlayerCoords(playerId, x, y, width, height);

            // 화면에 있는 cell x,y와 일치하는 CellView 찾기
            foreach (Transform child in boardRoot)
            {
                var cell = child.GetComponent<CellView>();
                //if (cell != null && cell.x == screenPos.x && cell.y == screenPos.y)
                if (cell != null && cell.x == x && cell.y == y)
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
                Sprite pieceSprite = GetSprite(piece.pieceType, piece.owner == model.GetPlayerId());
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

    public void ShowGameOver(bool isWin)
    {   
        if (isWin)
            winText.text = "You won!";
        else
            winText.text = "You lost!";
        gameOverPanel.SetActive(true);
    }

    public void OnGameOverButtonClicked()
    {
        SceneManager.LoadScene("GameSelectScene");
    }

    public void ShowAlert(string message)
    {
        if (alertText == null) return;
        alertText.text = message;
        alertText.gameObject.SetActive(true);

        // 기존 코루틴이 있다면 중복 방지
        StopCoroutine("HideAlertCoroutine");
        StartCoroutine(HideAlertCoroutine());
    }

    IEnumerator HideAlertCoroutine()
    {
        yield return new WaitForSeconds(3f);
        if (alertText != null)
            alertText.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameOverButton.onClick.AddListener(OnGameOverButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
