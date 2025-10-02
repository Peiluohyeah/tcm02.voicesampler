using System;
using UnityEngine;

public class AudioSynthesizer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioSource audioSource;

    [Header("Playback Mode")]
    [SerializeField] bool enableLoop = false;
    [SerializeField] bool enablePingPong = false;
    [SerializeField] bool pingpongCycled = false;

    [Header("Note Settings")]
    [Tooltip("The reference note of the audio clip (default: C3)")]
    public Note referenceNote = Note.C3;
    [Tooltip("Current note to play")]
    public Note currentNote = Note.C3;

    // Ping pong state
    [SerializeField] bool isPlayingForward = true;
    private float clipLength;




    // Musical notes enum
    public enum Note
    {
        C0, CS0, D0, DS0, E0, F0, FS0, G0, GS0, A0, AS0, B0,
        C1, CS1, D1, DS1, E1, F1, FS1, G1, GS1, A1, AS1, B1,
        C2, CS2, D2, DS2, E2, F2, FS2, G2, GS2, A2, AS2, B2,
        C3, CS3, D3, DS3, E3, F3, FS3, G3, GS3, A3, AS3, B3,
        C4, CS4, D4, DS4, E4, F4, FS4, G4, GS4, A4, AS4, B4,
        C5, CS5, D5, DS5, E5, F5, FS5, G5, GS5, A5, AS5, B5,
        C6, CS6, D6, DS6, E6, F6, FS6, G6, GS6, A6, AS6, B6,
        C7, CS7, D7, DS7, E7, F7, FS7, G7, GS7, A7, AS7, B7,
        C8
    }


    void Start()
    {
        // Setup AudioSource
        audioSource.clip = audioClip;
        audioSource.playOnAwake = false;

        if (audioClip != null)
        {
            clipLength = audioClip.length;
        }

        // Set initial pitch based on reference note
        UpdatePitch();
    }

    void Update()
    {

        if (enablePingPong)
        {
            HandlePingPong();
        }
    }


    void HandlePingPong()
    {
        if (audioSource == null || audioClip == null) return;

        if (pingpongCycled && !enableLoop) return;

        if (audioSource.isPlaying)
        {
            if (audioSource.time > clipLength - 0.1f && isPlayingForward)
            {
                audioSource.Stop();
                // Finished playing forward, now play backward
                isPlayingForward = false;
                // Use a value slightly less than clipLength to avoid seek error
                audioSource.time = Mathf.Max(0, clipLength - 0.01f);
                int semitoneOffset = (int)currentNote - (int)referenceNote;
                audioSource.pitch = -SemitonesToPitchRatio(semitoneOffset);
                audioSource.Play();
                pingpongCycled = true;
            }
            else if (audioSource.time < 0.1f && !isPlayingForward)
            {
                audioSource.Stop();
                isPlayingForward = true;
                audioSource.time = 0f;
                int semitoneOffset = (int)currentNote - (int)referenceNote;
                audioSource.pitch = SemitonesToPitchRatio(semitoneOffset);
                audioSource.Play();
            }
        }
    }

    void UpdatePitch()
    {
        if (audioSource != null)
        {
            // Calculate pitch based on semitone difference
            int semitoneOffset = (int)currentNote - (int)referenceNote;
            float pitchRatio = SemitonesToPitchRatio(semitoneOffset);

            // Apply pitch with correct direction for ping pong
            if (enablePingPong && !isPlayingForward)
            {
                audioSource.pitch = -pitchRatio;
            }
            else
            {
                audioSource.pitch = pitchRatio;
            }
        }
    }

    /// <summary>
    /// Convert semitone offset to pitch ratio
    /// Formula: pitch = 2^(semitones/12)
    /// Each semitone up multiplies frequency by 2^(1/12) ≈ 1.05946
    /// </summary>
    float SemitonesToPitchRatio(int semitones)
    {
        return Mathf.Pow(2f, semitones / 12f);
    }

    /// <summary>
    /// Get frequency for a given note (A4 = 440Hz reference)
    /// </summary>
    public float GetNoteFrequency(Note note)
    {
        // A4 is at index 57, frequency is 440Hz
        int semitonesFromA4 = (int)note - (int)Note.A4;
        return 440f * Mathf.Pow(2f, semitonesFromA4 / 12f);
    }

    // Public methods for playback control
    public void Play()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            isPlayingForward = true;
            audioSource.time = 0; 
            UpdatePitch();
            audioSource.Play();
            pingpongCycled = false;
        }
    }

    public void Stop()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            isPlayingForward = true;
            pingpongCycled = false;
        }
    }

    public void PlayNote(Note note)
    {
        currentNote = note;
        Play();
    }

    public void SetPingPongMode(bool enabled)
    {
        enablePingPong = enabled;
        if (enabled)
        {
            audioSource.loop = false; // Disable loop if ping pong is enabled
        }
        else
        {
            // Reset to forward playback
            isPlayingForward = true;
            audioSource.loop = enableLoop;
        }
    }

    public void SetLoopMode(bool enabled)
    {
        enableLoop = enabled;
        if (!enablePingPong)
        {
            audioSource.loop = enabled;
        }
    }

    /// <summary>
    /// Get current pitch ratio relative to reference note
    /// </summary>
    public float GetCurrentPitchRatio()
    {
        return audioSource != null ? Mathf.Abs(audioSource.pitch) : 1f;
    }

    /// <summary>
    /// Get current playback position normalized to [0.0f, 1.0f]
    /// </summary>
    public float PlaybackPosition
    {
        get
        {
            if (audioSource != null && audioClip != null && clipLength > 0)
            {
                return Mathf.Clamp01(audioSource.time / clipLength);
            };
            return 0.0f;
        }
    }

    /// <summary>
    /// Display pitch and frequency information
    /// </summary>
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 240));
        GUILayout.Label($"Reference Note: {referenceNote}");
        GUILayout.Label($"Current Note: {currentNote}");
        GUILayout.Label($"Pitch Ratio: {GetCurrentPitchRatio():F4}");
        GUILayout.Label($"Reference Frequency: {GetNoteFrequency(referenceNote):F2} Hz");
        GUILayout.Label($"Current Frequency: {GetNoteFrequency(currentNote):F2} Hz");
        GUILayout.Label($"Loop: {(enableLoop ? "Enabled" : "Disabled")}");
        GUILayout.Label($"Ping Pong: {(enablePingPong ? "Enabled" : "Disabled")}");
        GUILayout.Label($"Direction: {(isPlayingForward ? "Forward" : "Backward")}");
        GUILayout.Label($"Playback Position: {PlaybackPosition:F3}");
        GUILayout.EndArea();
    }
}