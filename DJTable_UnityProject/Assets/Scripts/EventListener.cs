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

    // Individual volumes for all 8 tracks in [0,1] range
    // Changed from VolumeControls
    public float[] trackVolumes = new float[8]{1f,1f,1f,1f,1f,1f,1f,1f};
    private float[] prevTrackVolumes = new float[8] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

    private Dictionary<int, ObjectInstrument> instrumentStates;
    private FMOD.Studio.EventInstance eventInstance;
    private FMOD.Studio.EventDescription eventDescription;
    private int numberOfActiveObjects = 0;


    void Start()
    {
        //SurfaceInputs.Instance.OnTouch += OnTouchReceive;
        SurfaceInputs.Instance.OnObjectAdd += OnObjectAddReceive;
        SurfaceInputs.Instance.OnObjectRemove += OnObjectRemoveReceive;
        SurfaceInputs.Instance.OnObjectUpdate += OnObjectUpdateReceive;
        DistanceEffectsController.Instance.OnGroupingChange += HandleGroupingChanges;

        eventDescription = FMODUnity.RuntimeManager.GetEventDescription(selectEvent);
        InstanciateDictionaries();
    }

    private void OnObjectUpdateReceive(List<ObjectInput> objects)
    {
        foreach (ObjectInput tableObject in objects)
        {
            ObjectInstrument objectInstrument = instrumentStates[tableObject.tagValue];
            if (!objectInstrument.isTriggered)
            {
                UpdateTrackValue(tableObject.tagValue, tableObject);
            }
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

    private void OnObjectAddReceive(List<ObjectInput> objects)
    {

        if(numberOfActiveObjects == 0)
        {
            eventDescription.createInstance(out eventInstance);
            eventInstance.start();
        }
        foreach(ObjectInput tableObject in objects)
        {
            prevTrackVolumes[tableObject.tagValue] = 0f;
            Debug.Log(instrumentStates[tableObject.tagValue].isTriggered);
            if (!instrumentStates[tableObject.tagValue].isTriggered)
            {
                UpdateTrackValue(tableObject.tagValue, tableObject);
            }
            UpdateAudioEffects(tableObject.tagValue, tableObject);
            numberOfActiveObjects += 1;
        }
        
    }

 

    void HandleGroupingChanges(List<ObjectInput> changedObjects)
    {
        foreach(ObjectInput changedObject in changedObjects)
        {
            ObjectInstrument objectInstrument = instrumentStates[changedObject.tagValue];
            if (DistanceEffectsController.Instance.objectsGrouped[objectInstrument.id] == true)
            {
                if (!objectInstrument.isTriggered)
                {
                    objectInstrument.Trigger();
                    eventInstance.setParameterByName("Volume" + objectInstrument.instrument, 1);
                    eventInstance.setParameterByName(objectInstrument.instrument, 5);
                    // if this instrument is changed, we should try to change all other instruments

                }
            }
            else
            {
                if (objectInstrument.isTriggered)
                {
                    objectInstrument.UnTrigger();
                    eventInstance.setParameterByName("Volume" + objectInstrument.instrument, objectInstrument.volume);
                    eventInstance.setParameterByName(objectInstrument.instrument, objectInstrument.trackValue);
                }
            }
        }
    }



     // Called to create the dictionnaries with the right values
     void InstanciateDictionaries()
    {
        instrumentStates = new Dictionary<int, ObjectInstrument>();
        instrumentStates.Add(0, new ObjectInstrument(0, "Kick"));
        instrumentStates.Add(1, new ObjectInstrument(1, "Snare"));
        instrumentStates.Add(4, new ObjectInstrument(4, "Bass"));
        instrumentStates.Add(3, new ObjectInstrument(3, "Percu"));
        instrumentStates.Add(2, new ObjectInstrument(2, "Hihat"));
        instrumentStates.Add(7, new ObjectInstrument(7, "Lead"));
        instrumentStates.Add(5, new ObjectInstrument(5, "Strings"));
        instrumentStates.Add(6, new ObjectInstrument(6, "Wind"));
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
        float flangerValue = x > flangerXMin ? (x-flangerXMin) / flangerXMin : 0;
        float distortionValue = y < distortionYMax ? (distortionYMax - y) / distortionYMax : 0;
        float filterValue = y > filterYMin ? (y - filterYMin) / (1 - filterYMin) : 0;

        //Debug.Log(distortionValue);
        eventInstance.setParameterByName("Distortion" + instrumentStates[parameterTag].instrument, distortionValue);
        eventInstance.setParameterByName("Reverb" + instrumentStates[parameterTag].instrument, reverbValue);
        
        eventInstance.setParameterByName("Filter" + instrumentStates[parameterTag].instrument, filterValue);
        eventInstance.setParameterByName("Flanger" + instrumentStates[parameterTag].instrument, flangerValue);
        instrumentStates[parameterTag].UpdateEffects(reverbValue, flangerValue, filterValue, distortionValue);
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
            // This following line is only for the object creation. As the check whether instr.isTriggered is not done, we untrigger it here in case it spawns near another fiducial
            instrumentStates[instrumentTag].UnTrigger();
        }
    }


    // Update is called once per frame
    void Update()
    {
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        for (int i = 0; i < trackVolumes.Length; i++)
        {
            if (trackVolumes[i] != prevTrackVolumes[i])
            {
                Debug.Log("Updating track " + i);
                ObjectInstrument objectInstrument = instrumentStates[i];
                objectInstrument.UpdateVolume(trackVolumes[i]);
                eventInstance.setParameterByName("Volume" + objectInstrument.instrument, trackVolumes[i]);
                prevTrackVolumes[i] = trackVolumes[i];
            }
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
