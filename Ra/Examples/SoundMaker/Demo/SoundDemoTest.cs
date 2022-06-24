using Ra;
using UnityEngine;

public class SoundDemoTest : MonoBehaviour
{
    public AudioSource audioSource;
    public SoundData soundData;
    public AnimationCurve enterCurve, exitCurve;
    [Range(0, 1)] public float speed;
    private void Start()
    {
        SoundMaker.Trail
            .Configure(audioSource, soundData)
            .SetVolumeTriggerCurve("Area", enterCurve, exitCurve)
            .Play("Music")
            .When(() => Input.GetButton("Fire1"))
            .Play("Shoot")
            .End()
            .FixedLoop(() =>
            {
                transform.position += Vectors.standardInputDeltaRawHorizontal * speed;
            });
    }
}
