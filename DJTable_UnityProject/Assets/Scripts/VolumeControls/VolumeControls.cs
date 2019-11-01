using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeControls : MonoBehaviour
{

    public GameObject volumeSliderPrefab;
    public Color[] sliderColors = new Color[8];

    private Dictionary<int, int> controlledObjects;
    private Dictionary<int, int> fingersUsed;

    // Object tag / prefab instance pair
    private Dictionary<int, GameObject> volumeSliderInstances;
    private Dictionary<int, SpriteRenderer> fillRenderers;

    private EventListener audioEventListener;

    private float lastUpdate = 0;

    // Start is called before the first frame update
    void Start()
    {
        controlledObjects = new Dictionary<int, int>();
        fingersUsed = new Dictionary<int, int>();
        volumeSliderInstances = new Dictionary<int, GameObject>();
        fillRenderers = new Dictionary<int, SpriteRenderer>();

        SurfaceInputs.Instance.OnObjectAdd += RenderNewVolumeSliders;
        SurfaceInputs.Instance.OnObjectUpdate += UpdateVolumeSliderPositions;
        SurfaceInputs.Instance.OnObjectRemove += RemoveVolumeSliders;

        SurfaceInputs.Instance.OnFingerAdd += CheckNewFingers;
        SurfaceInputs.Instance.OnFingerRemove += FreeObjectConstraints;
        SurfaceInputs.Instance.OnFingerUpdate += UpdateVolume;

        DistanceEffectsController.Instance.OnGroupingChange += HandleGroupingChanges;

        audioEventListener = GameObject.Find("EventListener").GetComponent<EventListener>();
    }

    void RenderSlider(ObjectInput obj)
    {
        Color color = sliderColors[obj.tagValue];

        GameObject volumeSliderInstance = Instantiate(volumeSliderPrefab, obj.position, Quaternion.identity);
        GameObject contour = volumeSliderInstance.transform.GetChild(0).gameObject;
        SpriteRenderer contourRenderer = contour.GetComponent<SpriteRenderer>();
        SpriteRenderer fillRenderer = contour.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();

        int sortingLayerID = SortingLayer.NameToID("Obj" + obj.tagValue);

        contourRenderer.color = color;
        fillRenderer.color = color;
        fillRenderer.sortingLayerID = sortingLayerID;
        fillRenderer.sortingOrder = (obj.tagValue * 2) + 1;

        SpriteMask fillMask = volumeSliderInstance.transform.GetChild(1).gameObject.GetComponent<SpriteMask>();
        fillMask.isCustomRangeActive = true;
        fillMask.frontSortingLayerID = sortingLayerID;
        fillMask.frontSortingOrder = (obj.tagValue * 2) + 1;
        fillMask.backSortingLayerID = sortingLayerID;
        fillMask.backSortingOrder = obj.tagValue * 2;

        SetVolumeSliderFill(volumeSliderInstance, audioEventListener.trackVolumes[obj.tagValue]);

        GameObject existingInstance;
        if (volumeSliderInstances.TryGetValue(obj.tagValue, out existingInstance))
        {
            Destroy(existingInstance);
            volumeSliderInstances.Remove(obj.tagValue);
        }

        volumeSliderInstances.Add(obj.tagValue, volumeSliderInstance);
        fillRenderers.Add(obj.tagValue, fillRenderer);
    }

    /// <summary>
    /// When new objects are added to the table, this renders the individual
    /// volume sliders for them.
    /// </summary>
    void RenderNewVolumeSliders(List<ObjectInput> addedObjects)
    {
        foreach (ObjectInput obj in addedObjects)
        {
            // Don't render for grouped objects
            if (DistanceEffectsController.Instance.objectsGrouped[obj.tagValue] == true)
            {
                continue;
            }

            RenderSlider(obj);
        }
    }

    /// <summary>
    /// When objects are moved on the table, this updates the corresponding
    /// volume slider positions.
    /// </summary>
    void UpdateVolumeSliderPositions(List<ObjectInput> updatedObjects)
    {
        foreach (ObjectInput obj in updatedObjects)
        {
            GameObject instance;
            if (volumeSliderInstances.TryGetValue(obj.tagValue, out instance))
            {
                instance.transform.position = obj.position;
            }
        }
    }

    /// <summary>
    /// When objects are removed from the table, this removed the volume sliders
    /// rendered next to them.
    /// </summmary>
    void RemoveVolumeSliders(List<ObjectInput> removedObjects)
    {
        foreach (ObjectInput obj in removedObjects)
        {
            Debug.Log("FINDING VOLUME SLIDER INSTANCE FOR OBJECT " + obj.tagValue);
            GameObject instance;
            if (volumeSliderInstances.TryGetValue(obj.tagValue, out instance))
            {
                Debug.Log("REMOVING INSTANCE FOR " + obj.tagValue);
                Destroy(instance);
                volumeSliderInstances.Remove(obj.tagValue);
                fillRenderers.Remove(obj.tagValue);
            }
        }
    }

    /// <summary>
    ///  Respond to grouping changes for objects. Destroy/show the volume sliders accordingly.
    /// </summary>
    void HandleGroupingChanges(List<ObjectInput> changedObjects)
    {
        foreach (ObjectInput obj in changedObjects)
        {
            GameObject instance;
            if (volumeSliderInstances.TryGetValue(obj.tagValue, out instance) && DistanceEffectsController.Instance.objectsGrouped[obj.tagValue] == true) {
                Destroy(instance);
                volumeSliderInstances.Remove(obj.tagValue);
                fillRenderers.Remove(obj.tagValue);
            }
            else if (DistanceEffectsController.Instance.objectsGrouped[obj.tagValue] == false)
            {
                RenderSlider(obj);
            }
         
        }
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


                Vector2 diff = finger.position - obj.position;
                float distance = diff.magnitude;
                float angle = Vector2.SignedAngle(Vector2.right, diff); // deg: -180 to +180

                if ((angle > 90 || angle < -90) && distance > 1.1 && distance < 1.75) {
                    // In the volume control activation zone
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
            int objectTag;
            if (fingersUsed.TryGetValue(finger.id, out objectTag))
            {
                controlledObjects.Remove(objectTag);
                fingersUsed.Remove(finger.id);
            }
        }
    }

    /// <summary>
    /// Update the volume sliders when any finger currently controlling
    /// some object's volume are moved
    /// </summary>
    void UpdateVolume(List<FingerInput> updatedFingers)
    {
        foreach (FingerInput finger in updatedFingers)
        {
            int objectTag;
            if (fingersUsed.TryGetValue(finger.id, out objectTag))
            {
                int objectId;
                if (SurfaceInputs.Instance.objectInstances.TryGetValue(objectTag, out objectId))
                {
                    ObjectInput obj = SurfaceInputs.Instance.surfaceObjects[objectId];

                    Vector2 diff = finger.position - obj.position;
                    float distance = diff.magnitude;
                    float angle = Vector2.SignedAngle(Vector2.right, diff); // deg: -180 to +180

                    if ((angle > 90 || angle < -90) && distance > 0.7 && distance < 2.5)
                    {
                        float fill = AngleToFraction(angle);
                        SetVolumeSliderFill(volumeSliderInstances[obj.tagValue], fill);

                        if (audioEventListener != null)
                        {
                            audioEventListener.trackVolumes[obj.tagValue] = fill;
                            fillRenderers[obj.tagValue].color = Color.Lerp(Color.white, sliderColors[obj.tagValue], fill);
                        }
                    }
                }

            }
        }
    }

    /// <summary>
    /// Positions the slider fill mask according the fill ([0,1] range)
    /// </summary>
    void SetVolumeSliderFill(GameObject sliderObject, float fill)
    {
        // Check VolumeSlider prefab, it has a child called FillMask
        // 120 and 4 degrees are just manually found values where the
        // fill mask either covers the fill completely or reveals it completely.
        // We then just interpolate between them according to the fill proportion.
        Transform maskTransform = sliderObject.transform.Find("FillMask");
        maskTransform.rotation = Quaternion.Slerp(Quaternion.Euler(0,0,120), Quaternion.Euler(0,0,4), fill);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            foreach (GameObject obj in volumeSliderInstances.Values)
            {
                Destroy(obj);
            }

            volumeSliderInstances.Clear();
            fillRenderers.Clear();
            fingersUsed.Clear();
            controlledObjects.Clear();

            GameObject[] killList = GameObject.FindGameObjectsWithTag("VolumeSliders");
            foreach (GameObject mofo in killList)
            {
                Destroy(mofo);
            }

            return;
        }

        lastUpdate += Time.deltaTime;
        if (lastUpdate < 0.1f) {
            return;
        }
        lastUpdate = 0;

        List<int> toRemove = new List<int>();
        foreach (KeyValuePair<int, GameObject> entry in volumeSliderInstances)
        {
            if (!SurfaceInputs.Instance.objectInstances.ContainsKey(entry.Key))
            {
                Destroy(entry.Value);
                toRemove.Add(entry.Key);
            }
        }

        foreach (int key in toRemove)
        {
            Debug.Log("CLEANUP: REMOVED VOLUME SLIDER");
            volumeSliderInstances.Remove(key);
            fillRenderers.Remove(key);
            controlledObjects.Remove(key);
        }

        // GameObject[] sliders = GameObject.FindGameObjectsWithTag("VolumeSliders");
        // if (sliders.Length > SurfaceInputs.Instance.objectInstances.Count)
        // {

        // }
    }


    /// <summary>
    /// Maps an angle from -120 to 120 degrees (supports -180 to -90 and 90 to 180) to fraction in range [0.0, 1.0]
    /// Angle values on the right hand side of the circle are not handled and this method
    /// shouldn't be used for them.
    /// </summary>
    private float AngleToFraction(float angle)
    {
        if (angle >= 90f)
        {
            return Mathf.Min(1.0f, 0.5f + ((180f - angle) / 120f)); 
        }
        else if (angle <= -90f)
        {
            return Mathf.Max(0.0f, 0.5f - ((-180f - angle) / -120f));
        }

        return 1.0f;
    }
}
