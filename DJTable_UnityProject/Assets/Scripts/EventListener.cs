﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EventListener : MonoBehaviour
{
    // Start is called before the first frame update

    public string selectEvent = "event:/Kick Snare";
    private Dictionary<int, string> parameterNames;

    private Dictionary<int, int> instrumentStates;
    private FMOD.Studio.EventInstance eventInstance;
    private FMOD.Studio.EventDescription eventDescription;
    private bool isPlaying;


    void Start()
    {
        SurfaceInputs.Instance.OnTouch += OnTouchReceive;
        eventDescription = FMODUnity.RuntimeManager.GetEventDescription(selectEvent);
        eventDescription.createInstance(out eventInstance);
        InstanciateDictionaries();
        isPlaying = false;
    }


    // Called to create the dictionnaries with the right values
    void InstanciateDictionaries()
    {
        instrumentStates = new Dictionary<int, int>();
        parameterNames = new Dictionary<int, string>();
        parameterNames.Add(0, "Kick");
        parameterNames.Add(1, "Snare");
        parameterNames.Add(2, "Bass");
        parameterNames.Add(3, "Hihat");
        parameterNames.Add(4, "Percu");
        parameterNames.Add(5, "Lead");
        parameterNames.Add(6, "Strings");
        parameterNames.Add(7, "Wind");
        foreach (int i in parameterNames.Keys)
        {
            instrumentStates.Add(i, 0);
        }

    }
    // Function called each frame when there is a touch
    void OnTouchReceive(Dictionary<int, FingerInput> surfaceFingers, Dictionary<int, ObjectInput> surfaceObjects)
    {
        Debug.ClearDeveloperConsole();
        ObjectInput[] instrumentCurrentObjects = new ObjectInput[parameterNames.Count];
        for (int i = 0; i < parameterNames.Count; i++)
        {
            instrumentCurrentObjects[i] = null;
        }
        TryStartEvent(surfaceObjects);
        foreach (KeyValuePair<int, ObjectInput> entry in surfaceObjects)
        {
            //
            //Debug.Log(entry.Value.orientation);
            instrumentCurrentObjects[entry.Value.tagValue] = entry.Value;
            
        }
        for (int i = 0; i < parameterNames.Count; i++)
        {
            //instrumentCurrentObjects[i] can be null
            UpdateTrackValue(i, instrumentCurrentObjects[i]);
            
            if(instrumentCurrentObjects[i]!=null)
            {
                UpdateAudioEffects(i, instrumentCurrentObjects[i]);
            }
            
        }


    }

    private void UpdateAudioEffects(int parameterTag, ObjectInput objectInput)
    {
        //Reverb
        float reverbValue = objectInput.posRelative.x < 0.5 ? 1 - objectInput.posRelative.x : 0;
        float distortionValue = objectInput.posRelative.x > 0.5 ? objectInput.posRelative.x : 0;
        eventInstance.setParameterByName("Distortion" + parameterNames[parameterTag], distortionValue);
        eventInstance.setParameterByName("Reverb" + parameterNames[parameterTag], distortionValue);
    }

    // This method compute the value of the track played of a specific instrument according to its position, to its rotation etc...
    // TO Be changed
    private int ComputeTrackValue(ObjectInput objectInput)
    {

        if (objectInput == null)
            return 0;

        int or = (int)objectInput.orientation;
        return or<=5 && or>=1 ?or:1;
    }

    // Update the corresponding parameter
    private void UpdateTrackValue(int instrumentTag, ObjectInput instrumentObject)
    {
        int trackValue = ComputeTrackValue(instrumentObject);
        if (trackValue != instrumentStates[instrumentTag])
        {
            Debug.Log(parameterNames[instrumentTag]);
            Debug.Log(trackValue);
            eventInstance.setParameterByName(parameterNames[instrumentTag], trackValue);
            instrumentStates[instrumentTag] = trackValue;
        }
    }

    // Update is called once per frame
    void Update()
    {
        TryStopEvent();
    }

    // Called to stop event if no instrument is playing
    void TryStopEvent()
    {
        if (isPlaying)
        {
            bool active = false;
            foreach (int stateValue in instrumentStates.Values)
            {
                if (stateValue != 0)
                {
                    active = true;
                    break;
                }
            }
            if (!active)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                isPlaying = false;
            }
        }
    }

    // Called to start event if the event is stopped and an instrument is playing
    void TryStartEvent(Dictionary<int, ObjectInput> surfaceObjects)
    {
        if (!isPlaying && surfaceObjects.Count > 0)
        {
            //eventDescription.createInstance(out eventInstance);
            eventInstance.start();
            isPlaying = true;
        }
    }
}
