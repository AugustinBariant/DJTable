﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInstrument
{
    public int id;
    public string instrument;
    // track 0 is inactive
    public int trackValue { get; private set; }
    private float reverbValue;
    private float flangerValue;
    private float filterValue;
    private float distortionValue;

    public ObjectInstrument(int id, string instrument)
    {
        this.id = id;
        this.instrument = instrument;
        this.trackValue = 0;
        this.reverbValue = 0;
        this.flangerValue = 0;
        this.filterValue = 0;
        this.distortionValue = 0;

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