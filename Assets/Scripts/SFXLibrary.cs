using System.Collections.Generic;
using UnityEngine;

public class SFXLibrary : MonoBehaviour
{
    public static SFXLibrary instance;

    public List<AudioClip> clips = new List<AudioClip>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public AudioClip GetClip(string name)
    {
        foreach (var clip in clips)
        {
            if (clip != null && clip.name == name)
                return clip;
        }

        Debug.LogWarning("SFX not found: " + name);
        return null;
    }
}