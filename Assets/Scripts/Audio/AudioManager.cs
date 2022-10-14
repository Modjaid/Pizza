using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SoundType
{
    testSound,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Range(0, 100)]
    public float generalVolume = 80;
    public AudioTrack[] audioTracks;

    private List<AudioSource> sourcePool = new List<AudioSource>();

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySound(SoundType sound)
    {
        AudioTrack audioTrack = GetAudioTrack(sound);

        if (audioTrack.CanPlay())
        {
            AudioSource pooledSource = null;
            bool sourceIsInPool = false;

            foreach (AudioSource clip in sourcePool)
            {
                if (clip == null) continue;

                if (clip.clip == audioTrack.audioClip[0])
                {
                    sourceIsInPool = true;
                    pooledSource = clip;
                    break;
                }
            }

            if (sourceIsInPool)
            {
                pooledSource.Play();
            }
            else
            {
                GameObject source = new GameObject("Audio Source");
                DontDestroyOnLoad(source);
                AudioSource audioSource = source.AddComponent<AudioSource>();
                float volume = generalVolume / 100 * audioTrack.volume / 100;

                audioSource.clip = audioTrack.audioClip[Random.Range(0, audioTrack.audioClip.Length)];
                audioSource.volume = volume;

                audioSource.Play();
                audioTrack.lastTimePlayed = Time.time;

                if (!sourcePool.Contains(audioSource))
                {
                    sourcePool.Add(audioSource);
                }
            }
        }
    }

    public void PlaySound(int soundIndex)
    {
        AudioTrack audioTrack = audioTracks[soundIndex];
        {
            if (audioTrack.CanPlay())
            {
                AudioSource pooledSource = null;
                bool sourceIsInPool = false;

                foreach (AudioSource clip in sourcePool)
                {
                    if (clip == null) continue;

                    if (clip.clip == audioTrack.audioClip[0])
                    {
                        sourceIsInPool = true;
                        pooledSource = clip;
                        break;
                    }
                }

                if (sourceIsInPool)
                {
                    pooledSource.Play();
                }
                else
                {
                    GameObject source = new GameObject("Audio Source");
                    DontDestroyOnLoad(source);
                    AudioSource audioSource = source.AddComponent<AudioSource>();
                    float volume = generalVolume / 100 * audioTrack.volume / 100;

                    audioSource.clip = audioTrack.audioClip[Random.Range(0, audioTrack.audioClip.Length)];
                    audioSource.volume = volume;

                    audioSource.PlayOneShot(audioTrack.audioClip[Random.Range(0, audioTrack.audioClip.Length)], volume);

                    audioTrack.lastTimePlayed = Time.time;

                    if (!sourcePool.Contains(audioSource))
                    {
                        sourcePool.Add(audioSource);
                    }
                }
            }
        }
    }

    /*
    public void StopSound(SoundType sound)
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in sources)
        {
            audioTrack AudioTrack = GetAudioTrack(sound);
            if (source.clip == AudioTrack.audioClip[0])
            {
                AudioTrack.lastTimePlayed = Time.time - AudioTrack.audioClip[0].length;
                Destroy(source.gameObject);
            }
        }
    }
    */

    private AudioTrack GetAudioTrack(SoundType sound)
    {
        foreach (AudioTrack track in audioTracks)
        {
            if (track.sound == sound)
            {
                return track;
            }
        }

        Debug.LogError("Sound not found.");
        return null;
    }
}

[System.Serializable]
public class AudioTrack
{
    public SoundType sound;
    public AudioClip[] audioClip;
    [Range(0, 100)]
    public float volume = 100;

    public bool preventOverlapping;
    [HideInInspector]
    public float lastTimePlayed;

    public bool CanPlay()
    {
        if (preventOverlapping)
        {
            if (Time.time > lastTimePlayed + audioClip[0].length || lastTimePlayed == 0)
            {
                return true;
            }

            else return false;
        }

        else return true;
    }
}
