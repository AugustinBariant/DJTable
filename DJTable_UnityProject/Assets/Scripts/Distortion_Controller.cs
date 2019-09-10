using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distortion_Controller : MonoBehaviour
{
    public string selectSound;
    public string selectSecondSound;
    [Range(0f, 1f)]
    public float dist;
    private FMOD.Studio.EventInstance eventInstance;
    private FMOD.Studio.EventDescription eventDescription;
    private FMOD.Studio.EventDescription secondEventDescription;

    // Start is called before the first frame update
    void Start()
    {
        eventDescription = FMODUnity.RuntimeManager.GetEventDescription(selectSound);
        secondEventDescription = FMODUnity.RuntimeManager.GetEventDescription(selectSecondSound);

    }

    // Update is called once per frame
    void Update()
    {
        UpdateParameters();
        eventInstance.getTimelinePosition(out int l);
        //Debug.Log(l%2000);
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Obstacle")
        {
            StartInstance();
            
        }
    }

   

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Obstacle")
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
        }
    }


    private void UpdateParameters()
    {
        eventInstance.setParameterByName("Distortion", dist);
    }

    private void StartInstance()
    {
        secondEventDescription.getInstanceList(out FMOD.Studio.EventInstance[] instanceList);
        eventDescription.createInstance(out eventInstance);
        if (instanceList.Length > 0)
        {
            instanceList[0].getTimelinePosition(out int pos);
            Debug.Log(pos % 2000);
            eventInstance.setTimelinePosition(1000);
            eventInstance.getTimelinePosition(out int b);
            Debug.Log(b);
        }

        
        eventInstance.setTimelinePosition(500);
        eventInstance.start();
        eventInstance.getTimelinePosition(out int a);
        Debug.Log(a);

    }
}
