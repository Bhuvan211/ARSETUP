using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("Voiceovers (Index 0: Sun ... Index 9: Pluto)")]
    [SerializeField] private AudioClip[] celestialVoiceovers;

    [Header("Moon Voiceovers")]
    [SerializeField] private AudioClip[] moonVoiceovers;

    [Header("Solar System Voiceovers")]
    [SerializeField] private AudioClip[] solarSystemVoiceovers;

    [Header("Satellite Voiceovers")]
    [SerializeField] private AudioClip[] satelliteVoiceovers;

    private void Awake()
    {
        // Auto-assign or add AudioSource component if missing
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    /// <summary>
    /// Plays the celestial voiceover matching the tour index (0 = Sun, 9 = Pluto).
    /// </summary>
    public void PlayPlanetAudio(int index)
    {
        PlayClipFromArray(celestialVoiceovers, index, "Celestial");
    }

    /// <summary>
    /// Plays the moon voiceover matching the index.
    /// </summary>
    public void PlayMoonAudio(int index)
    {
        PlayClipFromArray(moonVoiceovers, index, "Moon");
    }

    /// <summary>
    /// Plays the solar system voiceover matching the index.
    /// </summary>
    public void PlaySolarSystemAudio(int index)
    {
        PlayClipFromArray(solarSystemVoiceovers, index, "SolarSystem");
    }

    /// <summary>
    /// Plays the satellite voiceover matching the index.
    /// </summary>
    public void PlaySatelliteAudio(int index)
    {
        PlayClipFromArray(satelliteVoiceovers, index, "Satellite");
    }

    private void PlayClipFromArray(AudioClip[] clipArray, int index, string categoryName)
    {
        if (clipArray == null || clipArray.Length == 0)
        {
            Debug.LogWarning($"AudioManager: No {categoryName} voiceovers assigned in Inspector!");
            return;
        }

        if (index >= 0 && index < clipArray.Length)
        {
            if (clipArray[index] != null)
            {
                audioSource.clip = clipArray[index];
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"AudioManager: Missing AudioClip at index {index} in {categoryName}.");
            }
        }
        else
        {
            Debug.LogError($"AudioManager: Index {index} out of bounds for {categoryName} array (Length: {clipArray.Length}).");
        }
    }

    /// <summary>
    /// Returns true while the voiceover clip is currently playing.
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}