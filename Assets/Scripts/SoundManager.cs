using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AudioResource
{
    public string AudioName;
    public AudioClip AudioClip;
}
public class SoundManager : MonoBehaviour
{
    public List<AudioResource> AudioResources;
    private Dictionary<string, AudioResource> AudioByName = new Dictionary<string, AudioResource>();
    public AudioListener AudioListener { get; private set; }
    public AudioSource AudioSource;
    public AudioSource SoundEffectSource;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        AudioListener = GetComponent<AudioListener>();

        foreach (AudioResource resource in AudioResources)
        {
            AudioByName.Add(resource.AudioName, resource);
        }
    }

    public void PlayAudioSource(string audioName, bool loop=false, float delay=0f)
    {
        bool found = AudioByName.TryGetValue(audioName, out AudioResource audioResource);
        if(!found)
        {
            return;
        }

        AudioSource.clip = audioResource.AudioClip;
        AudioSource.loop = loop;
        AudioSource.PlayDelayed(delay);
    }

    public void PlaySoundEffectOneShot(string audioName)
    {
        bool found = AudioByName.TryGetValue(audioName, out AudioResource audioResource);
        if (!found)
        {
            return;
        }

        SoundEffectSource.PlayOneShot(audioResource.AudioClip);
    }

    public void PlaySoundEffectAtPoint(string audioName, Vector3 position)
    {
        bool found = AudioByName.TryGetValue(audioName, out AudioResource audioResource);
        if (!found)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(audioResource.AudioClip, position);
    }
}
