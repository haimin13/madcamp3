using UnityEngine;
using System.Collections;

public class CardFlipper : MonoBehaviour
{
    public GameObject front;
    public GameObject back;

    private bool isFlipped = false;
    private bool isAnimating = false;

    public void OnFlip()
    {
        if (!isAnimating)
            StartCoroutine(FlipAnimation());
    }

    IEnumerator FlipAnimation()
    {
        isAnimating = true;

        float duration = 0.3f;
        float elapsed = 0f;

        float startAngle = isFlipped ? 180f : 0f;
        float endAngle = isFlipped ? 0f : 180f;
        bool swapped = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            transform.localRotation = Quaternion.Euler(0, angle, 0);

            if (!swapped && t >= 0.5f)
            {
                front.SetActive(isFlipped);   // ¾Õ¸é ²ô±â
                back.SetActive(!isFlipped);   // µÞ¸é ÄÑ±â
                swapped = true;
            }

            yield return null;
        }

        transform.localRotation = Quaternion.Euler(0, endAngle, 0);
        isFlipped = !isFlipped;
        isAnimating = false;
    }

    private void Start()
    {
        front.SetActive(true);
        back.SetActive(false);
        transform.localRotation = Quaternion.identity;
    }
}
