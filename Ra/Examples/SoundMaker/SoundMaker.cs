using Ra.Trail;
using UnityEngine;

public class SoundMaker : TrailObject<SoundMaker>
{
    public AudioSource audioSource;
    private SoundData soundData;
    private Transform transform;

    public SoundMaker SetTransform(Transform newTransform)
    {
        After(() =>
        {
            transform = newTransform;
        });
        return this;
    }

    public SoundMaker Configure(AudioSource source, SoundData data)
    {
        After(() =>
        {
            audioSource = source;
            soundData = data;
            source.volume = soundData.startVolume; 
        });

        return this;
    }
    public SoundMaker Play(string clipName, AudioSource source = null)
    {
        After(() =>
        {
            AudioClip clip = default;
            foreach (var obj in soundData.sounds)
            {
                if (obj.name.Equals(clipName))
                {
                    clip = obj.clip;
                    break;
                }
            }

            if (clip != null)
            {
                if (source)
                {
                    source.PlayOneShot(clip);
                }
                else
                {
                    audioSource.PlayOneShot(clip);
                }
                
            }
        });
        return this;
    }
    
    public SoundMaker SetVolume(float volume)
    {
        After(() =>
        {
            audioSource.volume = volume;
        });
        return this;
    }


    public SoundMaker WaitForTriggerEnter(string tag)
    {
        var entered = false;
        Loop(() => {
            entered = CheckEntered(tag);
        }).Wait(() => entered);
            
        return this;
    }
    
    private bool CheckEntered(string tag)
    {
        var entered = false;
        Collider[] hitColliders = 
            Physics.OverlapSphere(transform == null 
                ? audioSource.transform.position 
                : transform.position, soundData.triggerRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(tag))
            {
                entered = true;
            }
        }

        return entered;
    }
    private float FindFirstKeyTime(AnimationCurve curve)
    {
        Keyframe lastKey = curve[0];
        return lastKey.time;
    }
    private float FindLastKeyTime(AnimationCurve curve)
    {
        Keyframe lastKey = curve[curve.length - 1];
        return lastKey.time;
    }
    
    private float FindTime(AnimationCurve curve)
    {
        return FindLastKeyTime(curve) - FindFirstKeyTime(curve);
    }
    
    public SoundMaker SetVolumeTriggerCurve(string tag, AnimationCurve enterCurve, AnimationCurve exitCurve)
    {
        Pick(() => audioSource.volume, out var curveTime);
        Loop(() =>
        {
            var entered = CheckEntered(tag);
            var currentCurve = entered ? enterCurve : exitCurve;
            if (entered)
            {
                var lastKeyTime = FindLastKeyTime(currentCurve);
                if (curveTime.value < lastKeyTime) curveTime.value += Time.deltaTime / FindTime(currentCurve);
            }
            else
            {
                var firstKeyTime = FindFirstKeyTime(currentCurve);
                if(curveTime.value > firstKeyTime) curveTime.value -= Time.deltaTime / FindTime(currentCurve);
            }
            
            var curvePosition = currentCurve.Evaluate(curveTime.value);
            if (curvePosition == 0) audioSource.volume = 0;
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, 
                curvePosition, 
                curvePosition);
        });
        return this;
    }
    
    

    public SoundMaker SetVolumeTrigger(string tag, float enterValue, float exitValue, float time)
    {
        Loop(() =>
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, 
                CheckEntered(tag) ? enterValue : exitValue, Time.deltaTime / time);
        });
        return this;
    }
    
    public SoundMaker SetVolumeSmoothly(float volume, float time)
    {
        Loop(() =>
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, volume, Time.deltaTime / time);
        });
        return this;
    }
}
