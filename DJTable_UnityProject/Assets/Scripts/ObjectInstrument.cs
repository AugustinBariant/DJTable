using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInstrument
{
    public int id;
    public string instrument;
    // track 0 is inactive
    public int trackValue { get; private set; }
    // isTriggered is true if the object is close to another object. In that case track 5 is active.
    public bool isTriggered { get; private set; } 
    private float reverbValue;
    private float flangerValue;
    private float filterValue;
    private float distortionValue;
    public float volume { get; private set; }

    public ObjectInstrument(int id, string instrument)
    {
        this.id = id;
        this.instrument = instrument;
        this.trackValue = 0;
        this.reverbValue = 0;
        this.flangerValue = 0;
        this.filterValue = 0;
        this.distortionValue = 0;
        this.volume = 1;
        this.isTriggered = false;

    }
    public ObjectInstrument(int id, string instrument, int trackValue, float reverbValue, float flangerValue, float filterValue, float distortionValue)
    {
        this.id = id;
        this.instrument = instrument;
        this.trackValue = trackValue;
        this.reverbValue = reverbValue;
        this.flangerValue = flangerValue;
        this.filterValue = filterValue;
        this.distortionValue = distortionValue;
        this.volume = 1;
        this.isTriggered = false;

    }
    public void Trigger()
    {
        this.isTriggered = true;
    }

    public void UnTrigger()
    {
        this.isTriggered = false;
    }

    public void UpdateVolume(float volume)
    {
        this.volume = volume;
    }

    public void UpdateTrackValue(int trackValue)
    {
        this.trackValue = trackValue;
    }

    public void UpdateEffects(float reverbValue, float flangerValue, float filterValue, float distortionValue)
    {
        this.reverbValue = reverbValue;
        this.flangerValue = flangerValue;
        this.filterValue = filterValue;
        this.distortionValue = distortionValue;
    }

}
