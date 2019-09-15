using System;
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

    public static SurfaceInputs Instance { get { return _instance; }}

    public bool dummyMode = false;

    private void Awake() {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public delegate void TouchReceiveHandler(
        Dictionary<int, FingerInput> surfaceFingers,
        Dictionary<int, ObjectInput> surfaceObjects);
    public event TouchReceiveHandler OnTouch;

    private Thread listenerThread;
    private UdpClient client;

    private IPEndPoint remoteEndpoint;

    private Dictionary<int, FingerInput> surfaceFingers;

    private Dictionary<int, ObjectInput> surfaceObjects;

    private Queue<OSCBundle> packetQueue = new Queue<OSCBundle>();
    private object queueLock = new object();

    private Camera mainCamera;

    void Start()
    {
        surfaceFingers = new Dictionary<int, FingerInput>();
        surfaceObjects = new Dictionary<int, ObjectInput>();

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

    private void Listen() {
        Debug.Log("UDP Listener started...");
        while (true) {
            try {
                byte[] receivedBytes = client.Receive(ref remoteEndpoint);
                
                if (receivedBytes.Length > 0) {
                    lock (queueLock) {
                        OSCBundle packet = (OSCBundle)OSCPacket.Unpack(receivedBytes);
                        packetQueue.Enqueue(packet);
                    }
                }

            } catch (Exception error) {
                Debug.LogError(error.ToString());
            }
        }
    }

    private void LogState() {
        Debug.ClearDeveloperConsole();
        if (surfaceFingers.Count > 0) { 
            Debug.Log(surfaceFingers.Count + " fingers:");
            foreach (KeyValuePair<int, FingerInput> entry in surfaceFingers) {
                Debug.Log(entry.Key + " @ " + entry.Value.position.ToString());
            }
        }

        if (surfaceObjects.Count > 0) {
            Debug.Log(surfaceObjects.Count + " objects:");
            foreach (KeyValuePair<int, ObjectInput> entry in surfaceObjects) {
                Debug.Log(entry.Key + ", tag: " + entry.Value.tagValue + " @ " + entry.Value.position.ToString());
            }
        }
    }

    private void ProcessCursorMessage(OSCMessage msg) {
        string msgType = msg.Values[0].ToString(); //   source / alive / set / fseq
        
        switch (msgType) {
            case "alive": {
                List<int> ids = new List<int>(surfaceFingers.Keys);
                foreach (int id in ids) {
                    if (!msg.Values.Contains(id)) {
                        surfaceFingers.Remove(id);
                    }
                }
                break;
            }
            case "set": {
                int id = (int)msg.Values[1];

                float x = (float)msg.Values[2];
                float y = 1f - (float)msg.Values[3];
                Vector2 posRelative = new Vector2(x, y);
                Vector2 position = ComputeWorldPosition(x, y);

                float xVel = (float)msg.Values[4];
                float yVel = (float)msg.Values[5];
                Vector2 velocity = new Vector2(xVel, yVel);

                float acc = (float)msg.Values[6];

                FingerInput surfaceFinger;
                if (surfaceFingers.TryGetValue(id, out surfaceFinger)) {
                    surfaceFinger.UpdateProps(position, posRelative, velocity, acc);
                } else {
                    surfaceFinger = new FingerInput(id, position, posRelative, velocity, acc);
                    surfaceFingers.Add(id, surfaceFinger);
                }
                break;
            }
        }
    } 

    private void ProcessObjectMessage(OSCMessage msg) {
        string msgType = msg.Values[0].ToString(); //   source / alive / set / fseq
        
        switch (msgType) {
            case "alive": {
                List<int> ids = new List<int>(surfaceObjects.Keys);
                foreach (int id in ids) {
                    if (!msg.Values.Contains(id)) {
                        surfaceObjects.Remove(id);
                    }
                }
                break;
            }
            case "set": {
                int id = (int)msg.Values[1];
                int tagValue = (int)msg.Values[2];

                float x = (float)msg.Values[3];
                float y = 1f - (float)msg.Values[4];

                Vector2 posRelative = new Vector2(x, y);
                Vector2 position = ComputeWorldPosition(x, y);

                float orientation = (float)msg.Values[5];

                float xVel = (float)msg.Values[6];
                float yVel = (float)msg.Values[7];
                Vector2 velocity = new Vector2(xVel, yVel);

                float angularVel = (float)msg.Values[8];
                float acc = (float)msg.Values[9];
                float angularAcc = (float)msg.Values[10];

                ObjectInput surfaceObject;
                if (surfaceObjects.TryGetValue(id, out surfaceObject)) {
                    surfaceObject.UpdateProps(position, posRelative, orientation, velocity, acc, angularVel, angularAcc);
                } else {
                    surfaceObject = new ObjectInput(id, tagValue, position, posRelative, orientation, velocity, acc, angularVel, angularAcc);
                    surfaceObjects.Add(id, surfaceObject);
                }
                break;
            }
        }
    }

    Vector3 ComputeWorldPosition(float x, float y)
    {
        Vector3 position = new Vector3((float)x * Screen.width, (float)y * Screen.height, mainCamera.nearClipPlane);
        return mainCamera.ScreenToWorldPoint(position);
    }


    void OnApplicationQuit() {
        listenerThread.Abort();
        client.Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (dummyMode)
        {
            sendDummyData();
        } else
        {
            if (packetQueue.Count > 0)
            {
                lock (packetQueue)
                {
                    foreach (OSCBundle packet in packetQueue)
                    {
                        if (packet != null)
                        {
                            foreach (OSCMessage msg in packet.Values)
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
                        }
                    }
                    packetQueue.Clear();
                }
                OnTouch(surfaceFingers, surfaceObjects);
                // LogState();
            }
        }
        
    }

    void sendDummyData()
    {
        if (surfaceObjects.Count == 0)
        {
            surfaceObjects.Add(0, new ObjectInput(0, 0, ComputeWorldPosition(0.4f, 0.3f), new Vector2(0.4f, 0.3f), 1f, new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(1, new ObjectInput(1, 1, ComputeWorldPosition(0.6f, 0.25f), new Vector2(0.6f, 0.25f), 1f, new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(2, new ObjectInput(2, 2, ComputeWorldPosition(0.4f, 0.8f), new Vector2(0.4f, 0.8f), 1f, new Vector2(0, 0), 0f, 0f, 0f));
            surfaceObjects.Add(3, new ObjectInput(3, 3, ComputeWorldPosition(0.8f, 0.3f), new Vector2(0.8f, 0.3f), 1f, new Vector2(0, 0), 0f, 0f, 0f));
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

        OnTouch(surfaceFingers, surfaceObjects);
    }
}
