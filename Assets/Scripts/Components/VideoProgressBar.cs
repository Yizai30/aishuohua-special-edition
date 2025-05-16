
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoProgressBar : MonoBehaviour, IDragHandler, IPointerDownHandler

{
    public bool draggable = true;
    public VideoPlayer videoPlayer;
    private Image progress;
    public Camera cam = null;

    public void PlayOrPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        } else
        {
            videoPlayer.Play();
        }
    }

    private void Awake()
    {
        progress = GetComponent<Image>();
    }
    private void Update()
    {
        if (videoPlayer.frameCount > 0) {
            progress.fillAmount = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (draggable) TrySkip(eventData);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (draggable) TrySkip(eventData);
    }
    private void TrySkip(PointerEventData eventData)
    {
        Vector2 localPoint;
        Debug.Log("position: " + eventData.position);
        Debug.Log("pressPosition: " + eventData.pressPosition);
        if (cam == null)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(progress.rectTransform, eventData.pressPosition, Camera.main, out localPoint))
            {
                float pct = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax, localPoint.x);
                Debug.Log("rect.xMin: " + progress.rectTransform.rect.xMin);
                Debug.Log("rect.xMax: " + progress.rectTransform.rect.xMax);
                Debug.Log("localPoint: " + localPoint.x);
                // Debug.Log(pct);
                SkipToPercent(pct);
            }
        } else
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(progress.rectTransform, eventData.pressPosition, cam, out localPoint))
            {
                float pct = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax, localPoint.x);
                Debug.Log("rect.xMin: " + progress.rectTransform.rect.xMin);
                Debug.Log("rect.xMax: " + progress.rectTransform.rect.xMax);
                Debug.Log("localPoint: " + localPoint.x);
                // Debug.Log(pct);
                SkipToPercent(pct);
            }
        }
    }

    private void SkipToPercent(float pct)
    {
        var frame = videoPlayer.frameCount * pct;
        videoPlayer.frame = (long)frame;
    }
}
