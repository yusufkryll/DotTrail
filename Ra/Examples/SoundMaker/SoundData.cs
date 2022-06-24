using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundData", menuName = "Ra/SoundData", order = 100)]
public class SoundData : ScriptableObject
{
    public float triggerRadius = 1f;
    [Range(0, 1)] public float startVolume = 1;
    public SoundObject[] sounds;
}

[Serializable]
public class SoundObject
{
    public string name;
    public AudioClip clip;
}
