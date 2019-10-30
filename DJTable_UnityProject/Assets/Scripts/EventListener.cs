using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EventListener : MonoBehaviour
{
    // Start is called before the first frame update

    public string selectEvent = "event:/Full tracks";
    public bool effectsOn = true;
    [Range(0.1f, 0.5f)]
    public float reverbXMax = 0.5f;
    [Range(0.5f, 0.9f)]
    public float flangerXMin = 0.5f;
    [Range(0.1f, 0.5f)]
    public float distortionYMax = 0.5f;
    [Range(0.5f, 0.9f)]
    public float filterYMin = 0.5f;

    private Dictionary<int, ObjectInstrument> instrumentStates;
    private FMOD.Studio.EventInstance eventInstance;
    private FMOD.Studio.EventDescription eventDescription;
    private int numberOfActiveObjects = 0;


    void Start()
    {
        //SurfaceInputs.Instance.OnTouch += OnTouchReceive;
        SurfaceInputs.Instance.OnObjectAdd += OnObjectAddRedceive;
        SurfaceInputs.Instance.OnObjectRemove += OnObjectRemoveReceive;
        SurfaceInputs.Instance.OnObjectUpdate += OnObjectUpdateReceive;

        eventDescription = FMODUnity.RuntimeManager.GetEventDescription(selectEvent);
        InstanciateDictionaries();
    }

    private void OnObjectUpdateReceive(List<ObjectInput> objects)
    {
        foreach (ObjectInput tableObject in objects)
        {
            ObjectInstrument objectInstrument = instrumentStates[tableObject.tagValue];
            UpdateTrackValue(tableObject.tagValue, tableObject);
            UpdateAudioEffects(tableObject.tagValue, tableObject);
        }
    }

    private void OnObjectRemoveReceive(List<ObjectInput> objects)
    {
        foreach (ObjectInput tableObject in objects)
        {
            ObjectInstrument objectInstrument = instrumentStates[tableObject.tagValue];
            UpdateTrackValue(tableObject.tagValue, null);
            numberOfActiveObjects -= 1;
        }

        if (numberOfActiveObjects == 0)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    private void OnObjectAddRedceive(List<ObjectInput> objects)
    {
        if(numberOfActiveObjects == 0)
        {
            eventDescription.createInstance(out eventInstance);
            eventInstance.start();
        }
        foreach(ObjectInput tableObject in objects)
        {
            UpdateTrackValue(tableObject.tagValue, tableObject);
            UpdateAudioEffects(tableObject.tagValue, tableObject);
            numberOfActiveObjects += 1;
        }
        
    }


    // Called to create the dictionnaries with the right values
    void InstanciateDictionaries()
    {
        instrumentStates = new Dictionary<int, ObjectInstrument>();
        instrumentStates.Add(0, new ObjectInstrument(0, "Kick"));
        instrumentStates.Add(1, new ObjectInstrument(1, "Snare"));
        instrumentStates.Add(2, new ObjectInstrument(2, "Bass"));
        instrumentStates.Add(3, new ObjectInstrument(3, "Hihat"));
        instrumentStates.Add(4, new ObjectInstrument(4, "Percu"));
        instrumentStates.Add(5, new ObjectInstrument(5, "Lead"));
        instrumentStates.Add(6, new ObjectInstrument(6, "Strings"));
        instrumentStates.Add(7, new ObjectInstrument(7, "Wind"));
    }
    // Function called each frame when there is a touch
    /*void OnTouchReceive(Dictionary<int, FingerInput> surfaceFingers, Dictionary<int, ObjectInput> surfaceObjects)
    {
        //Debug.ClearDeveloperConsole();
        ObjectInput[] instrumentCurrentObjects = new ObjectInput[instrumentStates.Count];
        for (int i = 0; i < instrumentStates.Count; i++)
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
        for (int i = 0; i < instrumentStates.Count; i++)
        {
            //instrumentCurrentObjects[i] can be null
            UpdateTrackValue(i, instrumentCurrentObjects[i]);
            
            if (effectsOn && instrumentCurrentObjects[i] != null)
            {
                UpdateAudioEffects(i, instrumentCurrentObjects[i]);
                Debug.Log("Audio Effect Updated");
            }

        }


    }
    */

    private void UpdateAudioEffects(int parameterTag, ObjectInput objectInput)
    {
        //Reverb
        float x = objectInput.posRelative.x;
        float y = objectInput.posRelative.y;

        float reverbValue = x < reverbXMax ? (reverbXMax - x) / reverbXMax : 0;
        float flangerValue = x > flangerXMin ? x / flangerXMin : 0;
        float distortionValue = y < distortionYMax ? (distortionYMax - y) / distortionYMax : 0;
        float filterValue = y > filterYMin ? (y - filterYMin) / (1 - filterYMin) : 0;

        //Debug.Log(distortionValue);
        eventInstance.setParameterByName("Distortion" + instrumentStates[parameterTag].instrument, distortionValue);
        eventInstance.setParameterByName("Reverb" + instrumentStates[parameterTag].instrument, reverbValue);
        instrumentStates[parameterTag].UpdateEffects(reverbValue, flangerValue, filterValue, distortionValue);
        //eventInstance.setParameterByName("Filter" + parameterNames[parameterTag], filterValue);
        //eventInstance.setParameterByName("Flanger" + parameterNames[parameterTag], flangerValue);
    }

    // This method compute the value of the track played of a specific instrument according to its position, to its rotation etc...
    // TO Be changed
    private int ComputeTrackValue(ObjectInput objectInput)
    {

        if (objectInput == null)
            return 0;

        //int or = (int)objectInput.orientation;
        //return or <= 5 && or >= 1 ? or : 1;
        float orientation = objectInput.orientation;
        return (int)(orientation / (Mathf.PI / 2f)) + 1;
    }

    // Update the corresponding parameter
    private void UpdateTrackValue(int instrumentTag, ObjectInput instrumentTableObject)
    {
        int trackValue = ComputeTrackValue(instrumentTableObject);
        if (trackValue != instrumentStates[instrumentTag].trackValue)
        {
            eventInstance.setParameterByName(instrumentStates[instrumentTag].instrument, trackValue);
            instrumentStates[instrumentTag].UpdateTrackValue(trackValue);
        }
    }


    // Update is called once per frame
    void Update()
    {
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        foreach(ObjectInstrument objectInstrument in instrumentStates.Values)
        {
            //line to change, put the right volume here
            float volume = 1;
            objectInstrument.UpdateVolume(volume);
            eventInstance.setParameterByName("Volume" + objectInstrument.instrument , volume);
        }
    }
    // Called to start event if the event is stopped and an instrument is playing
    /*void TryStartEvent(Dictionary<int, ObjectInput> surfaceObjects)
    {
        if (!isPlaying && surfaceObjects.Count > 0)
        {
            eventDescription.createInstance(out eventInstance);
            eventInstance.start();
            isPlaying = true;
        }
    }
    */
}
