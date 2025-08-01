using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShogiAnimation : MonoBehaviour
{
    public AudioClip drop; // Inspector에서 효과음 할당
    public AudioClip capture1;
    public AudioClip chack;
    public AudioSource audioSource; // 효과음을 재생할 오디오 소스
    public AnimationCurve easeCurve;
    public AnimationCurve easeStart;
    public GameObject trailPrefab;
    public GameObject shadowPrefab;
    public GameObject chackPrefab;
    public GameObject debrisPrefab;
    public GameObject dropPrefab;
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

    public void AfterAnimation()
    {
        Debug.Log("AfterAnimation");
        view.ShowPieces();
        view.RemoveHighlights();
        view.SetupCapturedPanels();
        view.ShowCapturedPieces();
        view.ShowGameOver();
    }

    public IEnumerator AnimateMove(List<int> from, List<int> to)
    {
        Debug.Log("AnimateMove 진입");
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

        if (audioSource != null && chack != null)
            audioSource.PlayOneShot(chack); // 애니메이션 시작과 동시에 재생

        rect.anchoredPosition = end;

        Vector2 pos = view.GetCellPos(to[0], to[1]);
        Vector2 startSize = rect.sizeDelta; // 현재 piece 사이즈

        yield return StartCoroutine(PlayScaleFadeEffect(chackPrefab, view.boardRoot, pos, startSize, 0.3f, 2f));


        yield return new WaitForSeconds(trailDuration);

        AfterAnimation();
    }
    public IEnumerator PlayScaleFadeEffect(GameObject effectPrefab, Transform parent, Vector2 anchoredPos, Vector2 startSize, float duration, float enlarge)
    {
        GameObject effect = Instantiate(effectPrefab, parent);
        RectTransform rt = effect.GetComponent<RectTransform>();
        Image img = effect.GetComponent<Image>();

        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = startSize;
        Color startColor = img.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // 크기 점점 키우기 (0 ~ 1.5배 예)
            rt.sizeDelta = Vector2.Lerp(startSize, startSize * enlarge, t);

            // 알파는 점점 0으로 줄이기
            img.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 끝났을 때 확실히 0크기, 투명으로 만들고 오브젝트 제거
        img.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Destroy(effect);
    }
    
    public IEnumerator AnimateCapture(List<int> from, List<int> to)
    {
        Debug.Log("AnimateCapture 진입");
        int fromX = from[0], fromY = from[1], toX = to[0], toY = to[1];

        GameObject movingPiece = view.pieceObjects[fromX, fromY];
        if (movingPiece == null) yield break;

        Vector2 start = view.GetCellPos(fromX, fromY);
        Vector2 end = view.GetCellPos(toX, toY);

        Vector2 stopover = GetExternalDivisionPoint(start, end, 1, 11);

        RectTransform rect = movingPiece.GetComponent<RectTransform>();
        movingPiece.transform.SetParent(view.boardRoot); // 보드 기준으로 위치 선정
        movingPiece.transform.SetAsLastSibling(); // 항상 최상위 레이어에!
        int movingPieceIndex = rect.GetSiblingIndex();

        Vector3 originalScale = Vector3.one;
        Vector3 enlargedScale = new Vector3(1.2f, 1.2f, 1f);

        // first Anim
        float duration = 0.5f;
        float elapsed = 0f;
        if (audioSource != null && capture1 != null)
            audioSource.PlayOneShot(capture1); // 애니메이션 시작과 동시에 재생
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

        // 잔상 파티클 생성 & siblingIndex 조절
        GameObject particle = null;
        if (debrisPrefab != null)
        {
            particle = GameObject.Instantiate(debrisPrefab, view.boardRoot);
            if (movingPieceIndex > 0)
                particle.transform.SetSiblingIndex(movingPieceIndex);   // piece 바로 아래로!
            else
                particle.transform.SetAsFirstSibling(); // 맨 아래로
            RectTransform particleRect = particle.GetComponent<RectTransform>();
            particleRect.anchoredPosition = end;
            particleRect.sizeDelta = rect.sizeDelta;
            


            GameObject.Destroy(particle, 1.0f); // 1초 뒤 자동 파괴
        }
        if (particle != null)
        {
            yield return new WaitForSeconds(1.0f);
        }

        AfterAnimation();
        yield break;
    }
    public IEnumerator AnimateDrop(List<int> to, Piece piece, int playerId)
    {
        Debug.Log("AnimateDrop 진입");
        int toX = to[0], toY = to[1];

        // Creating new pieceObject
        Vector2 end = view.GetCellPos(toX, toY);
        // Vector2 start = end + new Vector2(view.cellSize, view.cellSize);

        Transform parent = view.cellObjects[toX, toY].transform;
        var pieceObj = Instantiate(view.piecePrefab, parent);
        var pieceRt = pieceObj.GetComponent<RectTransform>();
        var pieceSize = view.cellSize * 0.86f;
        pieceRt.sizeDelta = new Vector2(pieceSize, pieceSize);
        pieceRt.anchoredPosition = Vector2.zero;
        pieceObj.SetActive(false);
        view.pieceObjects[toX, toY] = pieceObj;

        var shadowObj = Instantiate(shadowPrefab, parent);
        var shadowRt = shadowObj.GetComponent<RectTransform>();
        shadowRt.sizeDelta = new Vector2(view.cellSize, view.cellSize) * 0.9f;
        shadowRt.anchoredPosition = Vector2.zero;

        // 처음 크기 매우 작음
        Vector3 shadowStartScale = new Vector3(0.2f, 0.2f, 1f);
        Vector3 shadowEndScale = Vector3.one;
        shadowRt.localScale = shadowStartScale;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = easeStart.Evaluate(elapsed / duration);
            // 부드러운 확대(원한다면 가속도 등 곡선 적용 가능)
            shadowRt.localScale = Vector3.Lerp(shadowStartScale, shadowEndScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        shadowRt.localScale = shadowEndScale;
        Destroy(shadowObj);

        pieceObj.SetActive(true);
        var img = pieceObj.GetComponent<Image>();
        img.sprite = view.GetSprite(piece.pieceType, piece.owner == playerId);
        if (piece.owner != playerId)
            img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        
        Vector2 pos = view.GetCellPos(to[0], to[1]);
        Vector2 startSize = pieceRt.sizeDelta; // 현재 piece 사이즈

        if (audioSource != null && drop != null)
            audioSource.PlayOneShot(drop); // 애니메이션 시작과 동시에 재생

        yield return StartCoroutine(PlayScaleFadeEffect(dropPrefab, view.boardRoot, pos, startSize * 1.3f, 0.7f, 1.5f));

        yield return new WaitForSeconds(1.5f);

        AfterAnimation();
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
    }

    public Vector2 GetExternalDivisionPoint(Vector2 start, Vector2 end, float m, float n)
    {
        if (m != n)
        {
            return ((-n) * start + m * end) / (m - n);
        }
        else throw new System.ArgumentException("m=n");
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
