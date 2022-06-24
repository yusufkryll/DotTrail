using Ra.Subtitles;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleDemoTest : MonoBehaviour
{
    public Text text;
    public DefaultAsset subtitleData;
    public AudioSource audioSource;
    public AudioClip audioClip;
    private void Start()
    {
        SubtitleMaker.Trail
            .Configure(text)
            .After(() =>
            {
                audioSource.PlayOneShot(audioClip);
            })
            .ShowData(subtitleData);
    }
}
