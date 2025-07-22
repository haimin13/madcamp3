using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShogiAnimation : MonoBehaviour
{
    public AnimationCurve easeCurve;
    public AnimationCurve easeStart;
    public GameObject trailPrefab;
    public ShogiView view;
    public (List<int> from, List<int> to, string moveType)? GetMoveDelta(Piece[,] prev, Piece[,] curr)
    {
        if (prev == null) return null;

        List<int> from = null, to = null;
        string moveType = "";
        int width = prev.GetLength(0), height = prev.GetLength(1);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Piece prevPiece = prev[x, y];
                Piece curPiece = curr[x, y];
                if (prevPiece.owner != curPiece.owner)
                {
                    if (curPiece.owner != 0) // 01 02 12 21
                    {
                        to = new List<int> { x, y };
                        if (prevPiece.owner != 0) // 12 21
                            moveType = "caught";
                        else // 01 02
                            moveType = "move";
                    }
                    else // 10 20
                        from = new List<int>{ x, y };
                }
            }
        if (from == null)
            moveType = "drop";
        if (to != null)
            return (from, to, moveType);
        return null;
    }

    public IEnumerator AnimateMove(List<int> from, List<int> to)
    {
        int fromX = from[0], fromY = from[1], toX = to[0], toY = to[1];

        GameObject movingPiece = view.pieceObjects[fromX, fromY];
        if (movingPiece == null) yield break;

        Vector2 start = view.GetCellPos(fromX, fromY);
        Vector2 end = view.GetCellPos(toX, toY);

        RectTransform rect = movingPiece.GetComponent<RectTransform>();
        movingPiece.transform.SetParent(view.boardRoot); // 보드 기준으로 위치 선정
        movingPiece.transform.SetAsLastSibling(); // 항상 최상위 레이어에!
        int movingPieceIndex = rect.GetSiblingIndex();

        float duration = 0.5f;
        float trailInterval = 0.02f;
        float nextTrail = 0f;
        float elapsed = 0f;
        float trailDuration = 0.5f;

        while (elapsed < duration)
        {
            float t = easeCurve.Evaluate(elapsed / duration);
            Vector2 prevPos = rect.anchoredPosition;
            rect.anchoredPosition = Vector2.Lerp(start, end, t);

            if (elapsed >= nextTrail)
            {
                GameObject trail = Instantiate(trailPrefab, rect.parent); // boardRoot나 해당 cellObj
                if (movingPieceIndex > 0)
                    trail.transform.SetSiblingIndex(movingPieceIndex);   // piece 바로 아래로!
                else
                    trail.transform.SetAsFirstSibling(); // 맨 아래로
                RectTransform trailRect = trail.GetComponent<RectTransform>();
                // 피스와 동일한 위치와 사이즈 적용
                trailRect.sizeDelta = rect.sizeDelta;
                trailRect.anchoredPosition = prevPos;
                // ★ 페이드 코루틴 실행
                Image trailImg = trail.GetComponent<Image>();
                StartCoroutine(FadeTrailToBlack(trailImg, trailDuration)); // 0.5초간 점점 검정으로

                Destroy(trail, trailDuration);
                nextTrail += trailInterval;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = end;
        yield return new WaitForSeconds(trailDuration);
    }
    public Vector2 GetExternalDivisionPoint(Vector2 start, Vector2 end, float m, float n)
    {
        if (m != n)
        {
            return ((-n) * start + m * end) / (m - n);
        }
        else throw new System.ArgumentException("m=n");
    }
    
    public IEnumerator AnimateCapture(List<int> from, List<int> to)
    {
        int fromX = from[0], fromY = from[1], toX = to[0], toY = to[1];

        GameObject movingPiece = view.pieceObjects[fromX, fromY];
        if (movingPiece == null) yield break;

        Vector2 start = view.GetCellPos(fromX, fromY);
        Vector2 end = view.GetCellPos(toX, toY);

        Vector2 stopover = GetExternalDivisionPoint(start, end, 1, 11);

        RectTransform rect = movingPiece.GetComponent<RectTransform>();
        movingPiece.transform.SetParent(view.boardRoot); // 보드 기준으로 위치 선정
        movingPiece.transform.SetAsLastSibling(); // 항상 최상위 레이어에!

        Vector3 originalScale = Vector3.one;
        Vector3 enlargedScale = new Vector3(1.2f, 1.2f, 1f);

        // first Anim
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = easeStart.Evaluate(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(start, stopover, t);
            rect.localScale = Vector3.Lerp(originalScale, enlargedScale, t); 

            elapsed += Time.deltaTime;
            yield return null;
        }

        // second Anim
        duration = 0.8f;
        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = easeCurve.Evaluate(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(stopover, end, t);
            rect.localScale = Vector3.Lerp(enlargedScale, originalScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = end;
        rect.localScale = originalScale;
        yield break;
    }
    public IEnumerator AnimateDrop(List<int> to)
    {
        int toX = to[0], toY = to[1];

        yield break;
    }

    public IEnumerator FadeTrailToBlack(Image img, float duration)
    {
        Color startColor = img.color;
        Color endColor = new Color(0, 0, 0, 0); // 완전 투명 검은색
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (img == null || img.gameObject == null || !img.gameObject.activeInHierarchy)
                yield break;
            float t = elapsed / duration;
            img.color = Color.Lerp(startColor, endColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (img == null || img.gameObject == null || !img.gameObject.activeInHierarchy)
            yield break;
        img.color = endColor;
        yield break;
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
