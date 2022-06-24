using System.Collections;
using Ra.Trail;
using UnityEngine;
using UnityEngine.UI;

public class FadeMaker : TrailObject<FadeMaker>
{
    private Canvas canvas;
    private Image fadeImage;

    protected override void OnTrailStarted()
    {
        var go = new GameObject("FadeCanvas");
        go.AddComponent<Canvas>();

        canvas = go.GetComponent<Canvas>();
        canvas.sortingOrder = 9999;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        fadeImage = go.AddComponent<Image>();
        var c = Color.black;
        c.a = 0;
        fadeImage.color = c;
    }
    
    public FadeMaker SetColor(Color color)
    {
        After(() =>
        {
            var a = fadeImage.color.a;
            color.a = a;
            fadeImage.color = color;
        });
        return this;
    }
    public FadeMaker FadeIn(float duration)
    {
        While(() =>
        {
            var c = fadeImage.color;
            c.a = Mathf.MoveTowards(c.a, 1, Time.deltaTime / duration);
            fadeImage.color = c;
            return c.a < 0.95f;
        });
        return this;
    }
    public FadeMaker FadeOut(float duration)
    {
        While(() =>
        {
            var c = fadeImage.color;
            c.a = Mathf.MoveTowards(c.a, 0, Time.deltaTime / duration);
            fadeImage.color = c;
            return c.a > 0.05f;
        });
        return this;
    }
}
