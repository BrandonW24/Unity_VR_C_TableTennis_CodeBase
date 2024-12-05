using ML.SDK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JukeBox : MonoBehaviour
{
    public MLClickable backButton;
    public MLClickable forwardButton;
    public MLClickable pauseplaybutton;
    public MLClickable enableAutoPlaybutton;
    public MLClickable volumeUpButton;
    public MLClickable volumeDownButton;
    public Image autoplaybuttonimage; // For changing button color
    public Image pauseplaybuttonimage;

    public AudioSource Audioplayer;
    public AudioClip[] Songs;

    public GameObject[] Visualizer;
    public TextMeshPro SongTitle;
    [SerializeField] public Slider Progressbar; // Progress bar for song

    private int currentSongIndex = 0;
    private bool isPlaying = true;
    private bool autoplayEnabled = true; // Tracks autoplay state
    private float[] spectrumData = new float[64]; // Audio data for visualizer
    private float[] smoothedSpectrumData = new float[64]; // Smoothed spectrum data
    public float smoothingSpeed = 5f; // Smoothing factor for the visualizer
    private const float volumeStep = 0.1f; // Step for volume adjustments

    void OnBackButtonClick(MLPlayer player)
    {
        if (Songs.Length == 0) return;

        // Decrease the index and loop around if needed
        currentSongIndex = (currentSongIndex - 1 + Songs.Length) % Songs.Length;
        PlaySong(currentSongIndex);
        Debug.Log("Back button clicked. Playing song: " + Songs[currentSongIndex].name);
    }

    void OnForwardButtonClick(MLPlayer player)
    {
        if (Songs.Length == 0) return;

        // Increase the index and loop around if needed
        currentSongIndex = (currentSongIndex + 1) % Songs.Length;
        PlaySong(currentSongIndex);
        Debug.Log("Forward button clicked. Playing song: " + Songs[currentSongIndex].name);
    }

    void OnPausePlayButtonClick(MLPlayer player)
    {
        if (isPlaying)
        {
            Audioplayer.Pause();
            pauseplaybuttonimage.color = Color.red;
            Debug.Log("Paused playback.");
        }
        else
        {
            Audioplayer.Play();
            pauseplaybuttonimage.color = Color.green;
            Debug.Log("Resumed playback.");
        }
        isPlaying = !isPlaying;
    }

    void OnAutoPlayButtonClick(MLPlayer player)
    {
        autoplayEnabled = !autoplayEnabled; // Toggle autoplay

        // Change button color based on autoplay state
        autoplaybuttonimage.color = autoplayEnabled ? Color.green : Color.red;
        Debug.Log("Autoplay " + (autoplayEnabled ? "enabled" : "disabled"));
    }

    void OnVolumeUpButtonClick(MLPlayer player)
    {
        // Increase volume, but don't exceed 1
        Audioplayer.volume = Mathf.Clamp(Audioplayer.volume + volumeStep, 0, 1);
        Debug.Log("Volume increased. Current volume: " + Audioplayer.volume);
    }

    void OnVolumeDownButtonClick(MLPlayer player)
    {
        // Decrease volume, but don't go below 0
        Audioplayer.volume = Mathf.Clamp(Audioplayer.volume - volumeStep, 0, 1);
        Debug.Log("Volume decreased. Current volume: " + Audioplayer.volume);
    }

    void Start()
    {
        backButton.OnPlayerClick.AddListener(OnBackButtonClick);
        forwardButton.OnPlayerClick.AddListener(OnForwardButtonClick);
        pauseplaybutton.OnPlayerClick.AddListener(OnPausePlayButtonClick);
        enableAutoPlaybutton.OnPlayerClick.AddListener(OnAutoPlayButtonClick);
        volumeUpButton.OnPlayerClick.AddListener(OnVolumeUpButtonClick);
        volumeDownButton.OnPlayerClick.AddListener(OnVolumeDownButtonClick);

        if (Songs.Length > 0)
        {
            PlaySong(currentSongIndex);
        }

        // Initialize the progress bar if there's a song loaded
        if (Audioplayer.clip != null)
        {
            Progressbar.maxValue = Audioplayer.clip.length;
        }

        // Set default volume level
        Audioplayer.volume = 0.5f; // Set initial volume to 50%
    }

    void PlaySong(int index)
    {
        if (Songs.Length == 0 || index < 0 || index >= Songs.Length) return;

        Audioplayer.Stop(); // Stop the current song if playing
        Audioplayer.clip = Songs[index];
        Audioplayer.Play();
        isPlaying = true;

        // Update the song title
        SongTitle.text = Songs[index].name;

        // Set progress bar max value based on the song length
        Progressbar.maxValue = Audioplayer.clip.length;
        Progressbar.value = 0; // Reset progress bar
        Debug.Log("Now playing: " + Songs[index].name);
    }

    void Update()
    {
        // Update the progress bar with the current song time
        if (Audioplayer.clip != null && Audioplayer.isPlaying)
        {
            Progressbar.value = Audioplayer.time;
        }

        // Check if the song has ended and autoplay is enabled
        if (Audioplayer.clip != null && !Audioplayer.isPlaying && autoplayEnabled && isPlaying)
        {
            currentSongIndex = (currentSongIndex + 1) % Songs.Length;
            PlaySong(currentSongIndex);
        }

        // Get audio spectrum data to animate the visualizer
        Audioplayer.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

        for (int i = 0; i < Visualizer.Length; i++)
        {
            // Smooth out spectrum data by interpolating between current and previous values
            smoothedSpectrumData[i] = Mathf.Lerp(smoothedSpectrumData[i], spectrumData[i], Time.deltaTime * smoothingSpeed);

            // Scale the Y-axis of each visualizer object based on the smoothed audio data
            if (i < smoothedSpectrumData.Length)
            {
                float newYScale = Mathf.Lerp(0.1f, 5f, smoothedSpectrumData[i] * 100); // Adjust scaling factor as needed
                Vector3 newScale = Visualizer[i].transform.localScale;
                newScale.y = newYScale;
                Visualizer[i].transform.localScale = newScale;
            }
        }
    }



}
