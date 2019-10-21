using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeControls : MonoBehaviour
{

    private Dictionary<int, int> controlledObjects;
    private Dictionary<int, int> fingersUsed;

    // Start is called before the first frame update
    void Start()
    {
        controlledObjects = new Dictionary<int, int>();
        fingersUsed = new Dictionary<int, int>();

        SurfaceInputs.Instance.OnFingerAdd += CheckNewFingers;
        SurfaceInputs.Instance.OnFingerRemove = FreeObjectConstraints;
    }

    /// <summary>
    /// Checks newly added fingers to see if any of them
    /// fall into a volume control zone near one of the objects
    /// </summary>
    void CheckNewFingers(List<FingerInput> fingers)
    {
        // Objects currently on the table
        List<ObjectInput> objects = new List<ObjectInput>(SurfaceInputs.Instance.surfaceObjects.Values);

        foreach (FingerInput finger in fingers)
        {
            foreach (ObjectInput obj in objects)
            {
                if (controlledObjects.ContainsKey(obj.tagValue))
                {   
                    // If this object's volume is already being manipulated
                    // by some finger, skip it (one finger at a time please!)
                    continue;
                }


                Vector2 diff = finger.posRelative - obj.posRelative;
                float distance = diff.magnitude;
                float angle = Vector2.SignedAngle(obj.posRelative, finger.posRelative); // deg: -180 to +180

                if ((angle > 90 || angle < -90) && distance > 100 && distance < 200) {
                    // In the volume control activation zone
                    Debug.Log("IN VOLUME CONTROL @ distance " + distance + " with angle " + angle);

                    // Save state that this object is being manipulated by the newly added finger
                    controlledObjects.Add(obj.tagValue, finger.id);
                    fingersUsed.Add(finger.id, obj.tagValue);
                }
            }
        }
    }

    /// <summary>
    /// When fingers are removed from the table, this clears up the control state
    /// for the objects' volume controls.
    /// </summary>
    void FreeObjectConstraints(List<FingerInput> removedFingers)
    {
        foreach (FingerInput finger in removedFingers)
        {
            int tagValue;
            if (fingersUsed.TryGetValue(finger.id, out tagValue))
            {
                Debug.Log("REMOVED FINGER " + finger.id + " FROM OBJECT " + tagValue);
                controlledObjects.Remove(tagValue);
                fingersUsed.Remove(finger.id);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Converts an angle in degrees (-180 to -90 and 90 to 180) to fraction in range [0.0, 1.0]
    /// Angle values on the right hand side of the circle are not handled and this method
    /// shouldn't be used for them.
    /// </summary>
    private float AngleToFraction(float angle)
    {
        float fraction = 0.0f;
        if (angle >= 90f)
        {
            fraction = 90f / angle;
        }
        else if (angle <= -90f)
        {
            fraction = -90f / angle;
        }

        return fraction;
    }
}
