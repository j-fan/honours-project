using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatsFFT : MonoBehaviour {
    public AudioSource audioSource;
    float[] asamples = new float[64];
    public float avgFreq = 0.0f;
    public float runningAvgFreq = 0.0f;
    float alpha = 0.3f; //lowpass filter positions for smooth movement, lower number for more smoothing

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
    }

    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(asamples, 0, FFTWindow.Blackman);

        avgFreq = 0.0f;
        for (int i = 0; i < asamples.Length; i++)
        {
            avgFreq = avgFreq + asamples[i];
        }
        avgFreq = avgFreq * avgFreq;
        avgFreq = avgFreq / asamples.Length;
        runningAvgFreq = (avgFreq * alpha) + ((1 - alpha) * runningAvgFreq);

    }
}
