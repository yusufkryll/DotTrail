using UnityEngine;

public class FadeDemoTest : MonoBehaviour
{
    private void Start()
    {
        FadeMaker.Trail
            .SetColor(Color.black)
            .FadeIn(1)
            .Wait()
            .FadeOut(1);
    }
}
