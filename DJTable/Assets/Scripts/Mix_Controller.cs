using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class Mix_Controller : MonoBehaviour
{
    public string selectEvent;
    public string selectParameter;
    [Range(0f, 1f)]
    public float dist;
    private FMOD.Studio.EventInstance eventInstance;
    private FMOD.Studio.EventDescription eventDescription;
    private int activationState;
    // Start is called before the first frame update
    void Start()
    {
        eventDescription = FMODUnity.RuntimeManager.GetEventDescription(selectEvent);
        SetEventInstance();
        activationState = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLiveParameters();
        eventDescription.getInstanceList(out FMOD.Studio.EventInstance[] instanceList);
        Debug.Log(instanceList.Length);
    }

   //private void OnTriggerEnter2D(Collider2D collision)
   //{
   //    if (collision.tag == "Obstacle")
   //    {
   //        StartInstance();

   //    }
   //}



    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.tag == "Obstacle")
    //    {
    //        StopParameter();
    //    }
    //}


    private void UpdateLiveParameters()
    {
        eventInstance.setParameterByName("Distortion" + selectParameter, dist);
    }


    private void UpdateParameter()
    {
        eventInstance.setParameterByName(selectParameter, activationState);
    }

    private void SetEventInstance()
    {
        eventDescription.getInstanceList(out FMOD.Studio.EventInstance[] instanceList);
        if (instanceList.Length == 0)
        {
            eventDescription.createInstance(out eventInstance);
            eventInstance.start();
        }
        else
        {
            eventInstance = instanceList[0];
        }
        return;
    }

    private void OnMouseDown()
    {
        activationState = activationState < 2 ? activationState + 1 : 0;
        Debug.Log(activationState);
        UpdateParameter();
    }

}

