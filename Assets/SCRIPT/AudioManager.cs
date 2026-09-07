using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("Voiceovers (Index 0: Sun ... Index 9: Pluto)")]
    [SerializeField] private AudioClip[] celestialVoiceovers;

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
        if (celestialVoiceovers == null || celestialVoiceovers.Length == 0)
        {
            Debug.LogWarning("AudioManager: No voiceovers assigned in Inspector!");
            return;
        }

        if (index >= 0 && index < celestialVoiceovers.Length)
        {
            if (celestialVoiceovers[index] != null)
            {
                audioSource.clip = celestialVoiceovers[index];
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"AudioManager: Missing AudioClip at index {index}.");
            }
        }
        else
        {
            Debug.LogError($"AudioManager: Index {index} out of bounds for voiceovers array (Length: {celestialVoiceovers.Length}).");
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