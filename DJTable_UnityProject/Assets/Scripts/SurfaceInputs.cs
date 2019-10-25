﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

// Open Sound Control for .NET
// Copyright (c) 2006, Yoshinori Kawasaki 
using OSC.NET;

public class SurfaceInputs : MonoBehaviour
{
    private static SurfaceInputs _instance;
    public static SurfaceInputs Instance { get { return _instance; } }

    
    [Range(0.0f, (2 * Mathf.PI) - 0.001f)]
    public float[] rotations = new float[8];

    //Publicly accessible dictionaries
    //holding all objects and fingers that are currently on the surface
    public Dictionary<int, FingerInput> surfaceFingers { get; private set; }
    public Dictionary<int, ObjectInput> surfaceObjects { get; private set; }

    //All this variables are for the dummy mode. In that case, one can control the object with numbers "1" to "8" on the keyboard.
    public bool dummyMode = false;
    private int instrumentFocus = -1;
    private Dictionary<KeyCode, int> instrumentKeys = new Dictionary<KeyCode, int>();



    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    /**
     * Deprecated
     * remove this when no longer used.
     * If the whole dicts of objects/fingers needed, use SurfaceInputs.Instance.surfaceObjects
     * or SurfaceInputs.Instance.surfaceFingers respectively
     */
    public delegate void TouchReceiveHandler(
        Dictionary<int, FingerInput> surfaceFingers,
        Dictionary<int, ObjectInput> surfaceObjects);
    public event TouchReceiveHandler OnTouch;

    // The class publishes events when some object changes happen on the surface
    public delegate void ObjectChangeHandler(List<ObjectInput> objects);
    public event ObjectChangeHandler OnObjectAdd;
    public event ObjectChangeHandler OnObjectRemove;
    public event ObjectChangeHandler OnObjectUpdate;

    private Dictionary<int, float> removalTimes;

    private List<ObjectInput> lastAddedObjects;
    private List<ObjectInput> lastRemovedObjects;
    private List<ObjectInput> lastUpdatedObjects;

    private Thread listenerThread;
    private UdpClient client;

    private IPEndPoint remoteEndpoint;

    // We are only interested in the last received packet, no need to queue them.
    // Shared between the UDP client thread and the main thread,
    // so we use a lock to avoid weird things happening when both try to 
    // access and modify it.
    private OSCBundle lastPacket = null;
    private object packetLock = new object();

    private Camera mainCamera;

    void Start()
    {
        surfaceFingers = new Dictionary<int, FingerInput>();
        surfaceObjects = new Dictionary<int, ObjectInput>();

        removalTimes = new Dictionary<int, float>();

        lastAddedObjects = new List<ObjectInput>();
        lastRemovedObjects = new List<ObjectInput>();
        lastUpdatedObjects = new List<ObjectInput>();
        //For DummyMode
        instrumentKeys = InstantiateInstrumentKeys();



        mainCamera = Camera.main;

        // We will be listening to the Surface on localhost
        remoteEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3333);
        client = new UdpClient(remoteEndpoint);

        // Run the listener on a separate thread...
        ThreadStart threadStarter = new ThreadStart(Listen);
        listenerThread = new Thread(threadStarter);
        listenerThread.IsBackground = true;
        listenerThread.Start();
        
    }

    // A few comments on the TUIO protocol:
    // Whether it is a finger or object input depends on the address ("/tuio/2Dcur" or "/tuio/2Dobj" respectively)
    // Information is carried in two types of messages: "alive" and "set".
    // "alive" block lists the touch points that are currently active, there are no explicit messages
    // for touch point removal. This is due to the messages being sent via UDP, so technically unreliable.
    // We are communicating over localhost, so could in fact use pipes and introduce a custom protocol,
    // but not sure if it's worth rewriting now. The amount of touch points at a time is relatively small
    // to cause any data processing bottleneck, so probably no.


     //Run on a seperate thread,
     //listens to incoming UDP messages from the Surface in a loop.
     //When a packet is received, saves it as the last received packet.
    private void Listen()
    {
        Debug.Log("UDP Listener started...");
        while (true)
        {
            try
            {
                byte[] receivedBytes = client.Receive(ref remoteEndpoint);

                if (receivedBytes.Length > 0)
                {
                    lock (packetLock)
                    {
                        OSCBundle packet = (OSCBundle)OSCPacket.Unpack(receivedBytes);
                        // We don't care if there's an unprocessed packet already, we care about the latest only.
                        lastPacket = packet;
                    }
                }

            }
            catch (Exception error)
            {
                Debug.LogError(error.ToString());
            }
        }
    }

    private void LogState()
    {
        Debug.ClearDeveloperConsole();
        if (surfaceFingers.Count > 0)
        {
            Debug.Log(surfaceFingers.Count + " fingers:");
            foreach (KeyValuePair<int, FingerInput> entry in surfaceFingers)
            {
                Debug.Log(entry.Key + " @ " + entry.Value.position.ToString());
            }
        }

        if (surfaceObjects.Count > 0)
        {
            Debug.Log(surfaceObjects.Count + " objects:");
            foreach (KeyValuePair<int, ObjectInput> entry in surfaceObjects)
            {
                Debug.Log(entry.Key + ", tag: " + entry.Value.tagValue + " @ " + entry.Value.position.ToString());
            }
        }
    }

    /// <summary>
    /// Processes OSC messages of type "/tuio/2Dcur"
    /// </summary>
    private void ProcessCursorMessage(OSCMessage msg)
    {
        string msgType = msg.Values[0].ToString(); //   source / alive / set / fseq

        switch (msgType)
        {
            case "alive":
                {
                    //Alive message contains a list of finger IDs that are present on the table.
                    //Use it to see which ones are no longer there.
                    List<int> ids = new List<int>(surfaceFingers.Keys);
                    foreach (int id in ids)
                    {
                        if (!msg.Values.Contains(id))
                        {
                            surfaceFingers.Remove(id);
                        }
                    }
                    break;
                }
            case "set":
                {
                    // Set message contains the data for all fingers currently on the table.
                    // Use it for update and new finger addition.
                    int id = (int)msg.Values[1];

                    float x = (float)msg.Values[2];
                    float y = 1f - (float)msg.Values[3]; // y axis faces the opposite direction in Unity compared to what the Surface feeds

                    Vector2 posRelative = new Vector2(x, y); // relative positions in [0, 1] range
                    Vector2 position = ComputeWorldPosition(x, y); // absolute pixel positions

                    float xVel = (float)msg.Values[4];
                    float yVel = (float)msg.Values[5];
                    Vector2 velocity = new Vector2(xVel, yVel);

                    float acc = (float)msg.Values[6];

                    FingerInput surfaceFinger;
                    if (surfaceFingers.TryGetValue(id, out surfaceFinger))
                    {
                        surfaceFinger.UpdateProps(position, posRelative, velocity, acc);
                    }
                    else
                    {
                        surfaceFinger = new FingerInput(id, position, posRelative, velocity, acc);
                        surfaceFingers.Add(id, surfaceFinger);
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Processes OSC messages of type "/tuio/2Dobj"
    /// </summary>
    private void ProcessObjectMessage(OSCMessage msg)
    {
        string msgType = msg.Values[0].ToString(); //   source / alive / set / fseq

        switch (msgType)
        {
            case "alive":
                {
                    // Alive message contains a list of object IDs that are present on the table.
                    // Use it to see which ones are no longer there.
                    // We also populate the lastRemovedObjects list here to be published in the OnObjectRemove event
                    List<int> ids = new List<int>(surfaceObjects.Keys);
                    foreach (int id in ids)
                    {
                        if (!msg.Values.Contains(id) && !removalTimes.ContainsKey(id))
                        {
                            removalTimes.Add(id, Time.time);
                            // lastRemovedObjects.Add(surfaceObjects[id]);
                            // surfaceObjects.Remove(id);
                        }
                    }
                    break;
                }
            case "set":
                {
                    // Set message contains the data for all objects currently on the table.
                    // Use it for update (if changed) and new finger addition.
                    // We also populate the lastAddedObjects and lastUpdatedObjects here to be published
                    // in the respective events.
                    int id = (int)msg.Values[1];
                    int tagValue = (int)msg.Values[2];

                    float x = (float)msg.Values[3];
                    float y = 1f - (float)msg.Values[4]; // y axis faces the opposite direction in Unity compared to what the Surface feeds

                    Vector2 posRelative = new Vector2(x, y); // relative positions in [0, 1] range
                    Vector2 position = ComputeWorldPosition(x, y); // absolute pixel positions

                    float orientation = (float)msg.Values[5];

                    float xVel = (float)msg.Values[6];
                    float yVel = (float)msg.Values[7];
                    Vector2 velocity = new Vector2(xVel, yVel);

                    float angularVel = (float)msg.Values[8];
                    float acc = (float)msg.Values[9];
                    float angularAcc = (float)msg.Values[10];

                    ObjectInput surfaceObject;
                    if (surfaceObjects.TryGetValue(id, out surfaceObject))
                    {
                        // If object is already known (present in our dict), then we update it if its props has changed.
                        if (surfaceObject.posRelative != posRelative || surfaceObject.orientation != orientation)
                        {
                            surfaceObject.UpdateProps(position, posRelative, orientation, velocity, acc, angularVel, angularAcc);
                            lastUpdatedObjects.Add(surfaceObject);
                        }

                        if (removalTimes.ContainsKey(id)) {
                            removalTimes.Remove(id);
                        }
                    }
                    else
                    {
                        // If it's unknown (not in the dict), it's a new object so we add it.
                        surfaceObject = new ObjectInput(id, tagValue, position, posRelative, orientation, velocity, acc, angularVel, angularAcc);
                        surfaceObjects.Add(id, surfaceObject);
                        lastAddedObjects.Add(surfaceObject);
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Return pixel space coordinates, given [0,1] relative coordinates
    /// </summary>
    /// <param name="x">[0,1] x coordinate</param>
    /// <param name="y">[0,1] y coordinate</param>
    /// <returns>Pixel space coordinates</returns>
    Vector3 ComputeWorldPosition(float x, float y)
    {
        Vector3 position = new Vector3((float)x * Screen.width, (float)y * Screen.height, mainCamera.nearClipPlane);
        return mainCamera.ScreenToWorldPoint(position);
    }


    void OnApplicationQuit()
    {
        listenerThread.Abort();
        client.Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (dummyMode)
        {
            sendDummyData();
        }
        else
        {
            ProcessRemovalTimers();

            // If there's an unprocessed packet waiting, lock it and process
            if (lastPacket != null)
            {
                lock (packetLock)
                {
                    lastAddedObjects = new List<ObjectInput>();
                    //lastRemovedObjects = new List<ObjectInput>();
                    lastUpdatedObjects = new List<ObjectInput>();

                    foreach (OSCMessage msg in lastPacket.Values)
                    {
                        if (msg.Address.Equals("/tuio/2Dobj"))
                        {
                            ProcessObjectMessage(msg);
                        }
                        else if (msg.Address.Equals("/tuio/2Dcur"))
                        {
                            ProcessCursorMessage(msg);
                        }
                        // there's also /tuio/2Dblb
                        // but we don't really need it

                    }
                    lastPacket = null;
                }
                // Deprecated, remove when event no longer used
                OnTouch(surfaceFingers, surfaceObjects);


                // Publish the evens for added, removed and updated objects
                OnObjectAdd(lastAddedObjects);
                OnObjectRemove(lastRemovedObjects);
                OnObjectUpdate(lastUpdatedObjects);

                lastRemovedObjects.Clear();
                
                // LogState();
            }
            else if (lastRemovedObjects.Count > 0) {
                OnObjectRemove(lastRemovedObjects);
                lastRemovedObjects.Clear();
                
                OnTouch(surfaceFingers, surfaceObjects); // temp
            }
        }
    }

    void ProcessRemovalTimers() 
    {
        List<int> ids = new List<int>(removalTimes.Keys);
        foreach (int id in ids) {
            if (Time.time - removalTimes[id] >= 0.2f) {
                lastRemovedObjects.Add(surfaceObjects[id]);
                surfaceObjects.Remove(id);
                removalTimes.Remove(id);
            } 
        }
    }



    void sendDummyData()
    {
        foreach(KeyCode key in instrumentKeys.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                Debug.Log("Pressed " + key);
                instrumentFocus = instrumentKeys[key];
            }
        }
        //  If InstrumentFocus has been set, then we look for key pressed
        if (instrumentFocus != -1)
        {
            lastAddedObjects = new List<ObjectInput>();
            lastRemovedObjects = new List<ObjectInput>();
            lastUpdatedObjects = new List<ObjectInput>();
            bool deleted = false;
            //space is pressed to activate/desactivate the selected instrument
            if (Input.GetKeyDown("space"))
            {
                foreach(ObjectInput objectInput1 in surfaceObjects.Values)
                {
                    if (objectInput1.tagValue == instrumentFocus)
                    {
                        lastRemovedObjects.Add(objectInput1);
                        surfaceObjects.Remove(objectInput1.id);
                        OnObjectRemove(lastRemovedObjects);
                        deleted = true;
                        break;
                    }
                }
                if (!deleted)
                {
                    ObjectInput objectInput1 = new ObjectInput(instrumentFocus, instrumentFocus, ComputeWorldPosition(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0, new Vector2(0, 0), 0f, 0f, 0f);
                    surfaceObjects.Add(instrumentFocus, objectInput1);
                    lastAddedObjects.Add(objectInput1);
                    OnObjectAdd(lastAddedObjects);
                }
            }
            // If the object is already on the table, we can look for other keys used to control the position of the instrument
            if (surfaceObjects.TryGetValue(instrumentFocus, out ObjectInput objectInput))
            {
                bool modified = false;
                //up,down,left,right control the position
                if (Input.GetKey("up"))
                {
                    Vector2 newPos = objectInput.position + new Vector2(0, Time.deltaTime * 2f);
                    objectInput.UpdateProps(newPos, objectInput.posRelative, objectInput.orientation, objectInput.velocity, 0f, 0f, 0f);
                    modified = true;
                }
                if (Input.GetKey("down"))
                {
                    Vector2 newPos = objectInput.position + new Vector2(0, -Time.deltaTime * 2f);
                    objectInput.UpdateProps(newPos, objectInput.posRelative, objectInput.orientation, objectInput.velocity, 0f, 0f, 0f);
                    modified = true;
                }
                if (Input.GetKey("left"))
                {
                    Vector2 newPos = objectInput.position + new Vector2(-Time.deltaTime * 2f,0);
                    objectInput.UpdateProps(newPos, objectInput.posRelative, objectInput.orientation, objectInput.velocity, 0f, 0f, 0f);
                    modified = true;
                }
                if (Input.GetKey("right"))
                {
                    Vector2 newPos = objectInput.position + new Vector2(Time.deltaTime * 2f,0);
                    objectInput.UpdateProps(newPos, objectInput.posRelative, objectInput.orientation, objectInput.velocity, 0f, 0f, 0f);
                    modified = true;
                }
                if (Input.GetKey("a"))
                {
                    float newOrientation = -objectInput.orientation + Time.deltaTime * 2f;
                    objectInput.UpdateProps(objectInput.position, objectInput.posRelative, newOrientation, objectInput.velocity, 0f, 0f, 0f);
                    modified = true;
                }
                if (Input.GetKey("z"))
                {
                    float newOrientation = -objectInput.orientation - Time.deltaTime * 2f;
                    Debug.Log(objectInput.orientation);
                    Debug.Log(newOrientation);
                    objectInput.UpdateProps(objectInput.position, objectInput.posRelative, newOrientation, objectInput.velocity, 0f, 0f, 0f);
                    modified = true;
                }
                if (modified)
                {
                    lastUpdatedObjects.Add(objectInput);
                    OnObjectUpdate(lastUpdatedObjects);
                }
            }
        }

        /*if (surfaceObjects.Count == 0)
        {
            surfaceObjects.Add(0, new ObjectInput(0, 0, ComputeWorldPosition(0.4f, 0.3f), new Vector2(0.4f, 0.3f), rotations[0], new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(1, new ObjectInput(1, 1, ComputeWorldPosition(0.6f, 0.25f), new Vector2(0.6f, 0.25f), rotations[1], new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(2, new ObjectInput(2, 2, ComputeWorldPosition(0.4f, 0.6f), new Vector2(0.4f, 0.6f), rotations[2], new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(3, new ObjectInput(3, 3, ComputeWorldPosition(0.2f, 0.3f), new Vector2(0.2f, 0.3f), rotations[3], new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(4, new ObjectInput(4, 4, ComputeWorldPosition(0.3f, 0.7f), new Vector2(0.3f, 0.7f), rotations[4], new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(5, new ObjectInput(5, 5, ComputeWorldPosition(0.8f, 0.25f), new Vector2(0.8f, 0.25f), rotations[5], new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(6, new ObjectInput(6, 6, ComputeWorldPosition(0.5f, 0.8f), new Vector2(0.5f, 0.8f), rotations[6], new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(7, new ObjectInput(7, 7, ComputeWorldPosition(0.4f, 0.9f), new Vector2(0.4f, 0.9f), rotations[7], new Vector2(0, 0), 0f, 0f, 0f));

            List<ObjectInput> added = new List<ObjectInput>(surfaceObjects.Values);
            OnObjectAdd(added);
        } else
        {
            List<ObjectInput> updated = new List<ObjectInput>();
            for (int i = 0; i < rotations.Length; i++)
            {
                if (rotations[i] != surfaceObjects[i].orientation)
                {
                    surfaceObjects[i].UpdateOrientation(rotations[i]);
                    updated.Add(surfaceObjects[i]);
                }
            }

            // moving object
            ObjectInput obj = surfaceObjects[1];
            float x = obj.posRelative.x + (Time.deltaTime * 0.04f);
            float y = obj.posRelative.y - (Time.deltaTime * 0.04f);
            if (x >= 1.0f)
            {
                x = 0.0f;
            }
            if (y <= 0f)
            {
                y = 1.0f;
            }
            Vector2 position = ComputeWorldPosition(x, y);
            Vector2 posRelative = new Vector2(x, y);
            obj.UpdateProps(position, posRelative, 1f, new Vector2(0, 0), 0f, 0f, 0f);

            updated.Add(obj);
            
            if (updated.Count > 0)
            {
                OnObjectUpdate(updated);
            }
        }
        */

        OnTouch(surfaceFingers, surfaceObjects);
    }
    Dictionary<KeyCode, int> InstantiateInstrumentKeys()
    {
        Dictionary<KeyCode, int> instrumentKeys = new Dictionary<KeyCode, int>();
        instrumentKeys.Add(KeyCode.Alpha1, 0);
        instrumentKeys.Add(KeyCode.Alpha2, 1);
        instrumentKeys.Add(KeyCode.Alpha3, 2);
        instrumentKeys.Add(KeyCode.Alpha4, 3);
        instrumentKeys.Add(KeyCode.Alpha5, 4);
        instrumentKeys.Add(KeyCode.Alpha6, 5);
        instrumentKeys.Add(KeyCode.Alpha7, 6);
        instrumentKeys.Add(KeyCode.Alpha8, 7);
        return instrumentKeys;
    }
}
    