using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : NetworkBehaviour
{
    [Header("Volume")]
    //[Range(0f, 1f)] public float volume;
    
    [Header("Sound Clips")]
    public List<SoundClips> soundClips;
    
    [Header("Pitch Settings")]
    public bool randomPitch;
    public Vector2 pitchRange;
    
    [Header("Sound Distance Settings")]
    public bool useSoundDistance;
    public Vector2 soundDistance;

    private Dictionary<string, SoundClips> _soundLibrary;
    private AudioSource _audioSource;
    private int _lastIndex = -1;

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
        
        _soundLibrary = new Dictionary<string, SoundClips>();
        foreach (var soundClip in soundClips)
        {
            if (!_soundLibrary.ContainsKey(soundClip.key))
            {
                _soundLibrary.Add(soundClip.key, soundClip);
            }
        }
    }

    public void PlaySound(AudioClip clip, float volume)
    {
        _audioSource.clip = clip;
        
        if (randomPitch)
        {
            _audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        }
        
        if (useSoundDistance)
        {
            _audioSource.spatialBlend = 1;
            _audioSource.minDistance = soundDistance.x;
            _audioSource.maxDistance = soundDistance.y;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        
        _audioSource.volume = volume;
        _audioSource.Play();
    }
    
    public void PlayRandomGlobal(string key)
    {
        PlayRandomServerRpc(key);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayRandomServerRpc(string key)
    {
        PlayRandomObserversRpc(key);
    }

    [ObserversRpc]
    private void PlayRandomObserversRpc(string key)
    {
        PlayRandom(key); // uses your local logic
    }
    
    public void PlayRandom(string key)
    {
        if (_soundLibrary.TryGetValue(key, out var soundGroup))
        {
            PlayRandomSound(soundGroup.audioClips, soundGroup.volume);
        }
        else
        {
            Debug.LogWarning($"No sounds found for key: {key}");
        }
    }
    
    private void PlayRandomSound(AudioClip[] clips, float volume)
    {
        if (clips.Length == 0) return;
        
        // shuffle
        int index;
        do
        {
            index = Random.Range(0, clips.Length);
        } while (index == _lastIndex && clips.Length > 1);
        
        _lastIndex = index;
        _audioSource.clip = clips[index];
        
        if (randomPitch)
        {
            _audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        }

        if (useSoundDistance)
        {
            _audioSource.spatialBlend = 1;
            _audioSource.minDistance = soundDistance.x;
            _audioSource.maxDistance = soundDistance.y;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        
        _audioSource.volume = volume;
        _audioSource.Play();
    }
}

[Serializable]
public class SoundClips
{
    public string key;
    [Range(0f,1f)] public float volume;
    public AudioClip[] audioClips;
}
