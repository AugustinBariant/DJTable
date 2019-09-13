using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (AudioSource))]

public class Sampling : MonoBehaviour {

    // private attributes
    private AudioSource _audioSource;

    private float[] _bufferDecrease  = new float[8];
    private float[] _freqBandHighest = new float[8];

    private float _highestAmplitude;

    // public attributes
    public static float[] samplesLeft     = new float[512];
    public static float[] samplesRight    = new float[512];

    public static float[] freqBands       = new float[8];
    public static float[] bandBuffer      = new float[8];

    public static float[] audioBand       = new float[8];
    public static float[] audioBandBuffer = new float[8];   // will store a value [0, 1] for every band, which can be used for colors, lights,...

    public static float amplitude;
    public static float amplitudeBuffer;    // average amplitute of all bands

    public float audioProfile;

    public enum Channel { Stereo, Left, Right };
    public Channel channel = new Channel();

    // private methods
    private void Start ()
    {
        // getting the audio source component
        this._audioSource = GetComponent<AudioSource>();

        this.audioProfile = 5.0f;       // try with different values

        AudioProfile(audioProfile);     // sets the audio profile in order to avoid drastic changes at the beginning of the song, when the _freqBandHighest is still not set
    }
	
	private void Update ()
    {
        GetAudioSourceSpectrum();
        ComputeFrequencyBands();
        BandBuffer();
        CreateAudioBands();
        GetAmplitute();
    }

    private void GetAudioSourceSpectrum()
    {
        this._audioSource.GetSpectrumData(samplesLeft,  0, FFTWindow.Blackman);
        this._audioSource.GetSpectrumData(samplesRight, 1, FFTWindow.Blackman);
    }

    private void ComputeFrequencyBands()
    {
        /*
         * Standard frequency band division
         * 
         * 1) 20  -  60 Hz : Sub Bass
         * 2) 60  - 250 Hz : Bass
         * 3) 250 - 500 Hz : Low Midrange
         * 4) 500 -  2 kHz : Midrange
         * 5)   2 -  4 kHz : Upper Midrange
         * 6)   4 -  6 kHz : Presence
         * 7)   6 - 20 kHz : Brilliance
         * 
         * 20'000 Hz / 512 samples ~ 40 Hz per sample
         * 
         * Frequency bands that will be computed: 
         * 0)   2 samples =    86 Hz =>     0 -    86 Hz
         * 1)   4 samples =   172 Hz =>    87 -   258 Hz
         * 2)   8 samples =   344 Hz =>   259 -   602 Hz
         * 3)  16 samples =   688 Hz =>   603 -  1290 Hz
         * 4)  32 samples =  1376 Hz =>  1291 -  2666 Hz
         * 5)  64 samples =  2752 Hz =>  2667 -  5418 Hz
         * 6) 128 samples =  5504 Hz =>  5419 - 10922 Hz
         * 7) 256 samples = 11008 Hz => 10923 - 21930 Hz
         * 
         * we will "manually" add two more samples to the last band in order to cover the whole 22kHz spectrum
         */

        int count = 0;

        for (int i=0; i<8; i++)
        {
            float average = 0.0f;
            int sampleCount = (int)Mathf.Pow(2, i + 1);

            if(i == 7)
            {
                sampleCount += 2;
            }

            for(int j=0; j<sampleCount; j++)
            {
                if (channel == Channel.Stereo)
                {
                    average += (samplesLeft[count] + samplesRight[count]) * (count + 1);      // WHYYYY *(count + 1)
                }
                else if(channel == Channel.Left)
                {
                    average += samplesLeft[count] * (count + 1);      // WHYYYY *(count + 1)
                }
                else    // right channel
                {
                    average += samplesRight[count] * (count + 1);      // WHYYYY *(count + 1)
                }
                count++;
            }

            average /= count;

            Sampling.freqBands[i] = average * 10;
        }
    }

    private void BandBuffer()
    {
        for(int i=0; i<8; ++i)
        {
            if(freqBands[i] > bandBuffer[i])
            {
                bandBuffer[i] = freqBands[i];
                this._bufferDecrease[i] = 0.005f;
            }

            if (freqBands[i] < bandBuffer[i])
            {
                bandBuffer[i] -= this._bufferDecrease[i];
                this._bufferDecrease[i] *= 1.2f;
            }
        }
    }

    private void CreateAudioBands()
    {
        for(int i=0; i<8; i++)
        {
            if (freqBands[i] > this._freqBandHighest[i])
            {
                this._freqBandHighest[i] = freqBands[i];
            }

            audioBand[i]       = freqBands[i]  / this._freqBandHighest[i];
            audioBandBuffer[i] = bandBuffer[i] / this._freqBandHighest[i];
        }
    }

    private void GetAmplitute()
    {
        float currentAmplitude       = 0.0f;
        float currentAmplitudeBuffer = 0.0f;

        for(int i=0; i<8; i++)
        {
            currentAmplitude       += audioBand[i];
            currentAmplitudeBuffer += audioBandBuffer[i];
        }

        if (currentAmplitude > this._highestAmplitude)
        {
            this._highestAmplitude = currentAmplitude;
        }

        amplitude = currentAmplitude / this._highestAmplitude;
        amplitudeBuffer = currentAmplitudeBuffer / this._highestAmplitude;
    }

    private void AudioProfile(float audioProfile)
    {
        for (int i=0; i<8; i++)
        {
            this._freqBandHighest[i] = audioProfile;
        }
    }
}
