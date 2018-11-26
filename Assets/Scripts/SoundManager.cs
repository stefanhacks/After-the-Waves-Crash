using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance = null;

    public AudioSource efxSource, overlaySource, whiteNoiseSource;
    public AudioClip[] overlaySequence;

    public float lowPitchRange = .95f;
    public float highPitchRange = 1.05f;
    
    private int currentOverlay = 0;
    private bool playNext, isLast;

    private void Update()
    {
        if (playNext)
        { 
            if (overlaySource.isPlaying && overlaySource.loop)
                overlaySource.loop = false;
            else if (!overlaySource.isPlaying)
            {                    
                overlaySource.clip = overlaySequence[currentOverlay];
                overlaySource.loop = !isLast;
                overlaySource.Play();
                
                playNext = false;
            }
        }
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
    }

    public void WakeUp()
    {
        StartCoroutine(FadeIn(whiteNoiseSource, 1, 1));
    }

    public void PlaySingle(AudioClip clip)
    {
        efxSource.clip = clip;
        efxSource.Play();
    }

    public void RandomizeSfx(params AudioClip[] clips)
    {
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        efxSource.pitch = randomPitch;
        efxSource.clip = clips[randomIndex];
        efxSource.Play();
    }

    public void PlayOverlay(int overlayToPlay, bool last = false)
    {
        if (overlaySource.volume == 0) StartCoroutine(FadeIn(overlaySource, 0.9f, 1));
        currentOverlay = overlayToPlay;
        isLast = last;

        playNext = true;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float targetVolume, float fadeSpeed)
    {
        while (audioSource.volume < targetVolume)
        {
            float tempVolume = audioSource.volume + Time.deltaTime * fadeSpeed;
            audioSource.volume = Mathf.Min(tempVolume, targetVolume);
            yield return null;
        }
    }
}