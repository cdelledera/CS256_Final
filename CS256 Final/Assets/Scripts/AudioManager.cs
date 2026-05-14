using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("The Speakers")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Your Audio Library")]
    public Sound[] backgroundMusic;
    public Sound[] soundEffects;

    private Coroutine currentMusicFade;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // ==========================================
    // --- MUSIC LOGIC ---
    // ==========================================

    public void PlayMusic(string trackName)
    {
        Sound s = Array.Find(backgroundMusic, sound => sound.name == trackName);
        if (s == null) return;

        if (currentMusicFade != null) StopCoroutine(currentMusicFade);

        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.pitch = s.pitch;
        musicSource.loop = true;
        musicSource.Play();
    }

    // --- NEW: The Smooth Transition Function ---
    public void CrossfadeMusic(string trackName, float fadeDuration = 1.5f)
    {
        Sound s = Array.Find(backgroundMusic, sound => sound.name == trackName);
        if (s == null)
        {
            Debug.LogWarning("AudioManager: Couldn't find music track named " + trackName);
            return;
        }

        if (currentMusicFade != null) StopCoroutine(currentMusicFade);
        currentMusicFade = StartCoroutine(DoCrossfade(s, fadeDuration));
    }

    private IEnumerator DoCrossfade(Sound newSound, float duration)
    {
        float startVolume = musicSource.volume;
        float halfDuration = duration / 2f;

        // 1. Fade the current music out
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * (Time.deltaTime / halfDuration);
            yield return null;
        }

        // 2. Swap the track and set pitch
        musicSource.clip = newSound.clip;
        musicSource.pitch = newSound.pitch;
        musicSource.Play();

        // 3. Fade the new music in to its target volume
        while (musicSource.volume < newSound.volume)
        {
            musicSource.volume += newSound.volume * (Time.deltaTime / halfDuration);
            yield return null;
        }

        musicSource.volume = newSound.volume;
    }

    // ==========================================
    // --- SFX LOGIC ---
    // ==========================================

    public void PlaySFX(string sfxName)
    {
        Sound s = Array.Find(soundEffects, sound => sound.name == sfxName);
        if (s == null) return;

        sfxSource.pitch = s.pitch;
        sfxSource.PlayOneShot(s.clip, s.volume);
    }
}