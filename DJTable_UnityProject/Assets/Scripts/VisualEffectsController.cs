using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisualEffectsController : MonoBehaviour
{
    public GameObject[] prefabs = new GameObject[4];

    private Dictionary<int, GameObject> objectPrefabs;
    private Dictionary<int, GameObject> effectInstances;

    // Start is called before the first frame update
    void Start()
    {
        objectPrefabs = new Dictionary<int, GameObject>();
        effectInstances = new Dictionary<int, GameObject>();

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] != null)
            {
                objectPrefabs.Add(i, prefabs[i]);
            }
        }

        SurfaceInputs.Instance.OnObjectAdd += AddNewObjects;
        SurfaceInputs.Instance.OnObjectUpdate += UpdateObjects;
        SurfaceInputs.Instance.OnObjectRemove += RemoveObjects;
    }

    void AddNewObjects(List<ObjectInput> addedObjects)
    {
        foreach (ObjectInput addedObject in addedObjects)
        {
            int tagValue = addedObject.tagValue;
            GameObject prefab;
            if (objectPrefabs.TryGetValue(tagValue, out prefab) && !effectInstances.ContainsKey(tagValue))
            {
                GameObject instance = Instantiate(prefab, addedObject.position, Quaternion.identity);
                effectInstances.Add(tagValue, instance);
            }
        }
    }

    void UpdateObjects(List<ObjectInput> updatedObjects)
    {
        foreach (ObjectInput updatedObject in updatedObjects)
        {
            GameObject instance;
            if (effectInstances.TryGetValue(updatedObject.tagValue, out instance))
            {
                instance.transform.localPosition = updatedObject.position;
                //instance.transform.localRotation = Quaternion.Euler(0, 0, -entry.Value.orientation * Mathf.Rad2Deg);
            }
        }
    }

    void RemoveObjects(List<ObjectInput> removedObjects)
    {
        foreach (ObjectInput removedObject in removedObjects)
        {
            GameObject instance;
            if (effectInstances.TryGetValue(removedObject.tagValue, out instance))
            {
                Destroy(instance);
                effectInstances.Remove(removedObject.tagValue);
            }
        }
    }

}
