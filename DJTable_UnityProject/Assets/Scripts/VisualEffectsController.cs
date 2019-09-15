using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisualEffectsController : MonoBehaviour
{
    public GameObject[] prefabs = new GameObject[4];

    private Dictionary<int, GameObject> objectPrefabs;
    private Dictionary<int, GameObject> effectInstances;

    private Dictionary<int, FingerInput> surfaceFingers = new Dictionary<int,FingerInput>();
    private Dictionary<int, ObjectInput> surfaceObjects = new Dictionary<int,ObjectInput>();

    // Start is called before the first frame update
    void Start()
    {
        SurfaceInputs.Instance.OnTouch += ProcessObjects;

        objectPrefabs = new Dictionary<int, GameObject>();
        effectInstances = new Dictionary<int, GameObject>();

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] != null)
            {
                objectPrefabs.Add(i, prefabs[i]);
            }
        }

    }

    void ProcessObjects(Dictionary<int, FingerInput> surfaceFingers, Dictionary<int, ObjectInput> surfaceObjects)
    {
        this.surfaceFingers = surfaceFingers;
        this.surfaceObjects = surfaceObjects;
    }

    // Update is called once per frame
    void Update()
    {
        List<int> existantIds = new List<int>();
        foreach (KeyValuePair<int, ObjectInput> entry in surfaceObjects)
        {
            int tagValue = entry.Value.tagValue;
            existantIds.Add(tagValue);

            GameObject prefab;
            if (objectPrefabs.TryGetValue(tagValue, out prefab))
            {
                Vector2 position = entry.Value.position;
                GameObject instance;
                if (effectInstances.TryGetValue(tagValue, out instance))
                {
                    instance.transform.localPosition = position;
                    //instance.transform.localRotation = Quaternion.Euler(0, 0, -entry.Value.orientation * Mathf.Rad2Deg);
                }
                else
                {
                    instance = Instantiate(prefab, position, Quaternion.identity);
                    effectInstances.Add(tagValue, instance);
                }

            }
        }

        for (int i = 0; i < objectPrefabs.Count; i++)
        {
            if (!existantIds.Contains(i))
            {
                //Debug.Log("Destroy!");
                GameObject instance;
                if (effectInstances.TryGetValue(i, out instance))
                {
                    Destroy(instance);
                    effectInstances.Remove(i);
                }
            }
        }
    }
}
