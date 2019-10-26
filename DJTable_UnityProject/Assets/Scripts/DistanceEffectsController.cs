using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEffectsController : MonoBehaviour
{
    public GameObject expoldingPrefab;
    private Dictionary<int, GameObject> distanceInstances;


    void Start()
    {

        SurfaceInputs.Instance.OnObjectAdd += AddNewObjects;
        SurfaceInputs.Instance.OnObjectUpdate += UpdateObjects;
        SurfaceInputs.Instance.OnObjectRemove += RemoveObjects;
    }

    void AddNewObjects(List<ObjectInput> addedObjects)
    {
        foreach (ObjectInput addedObject in addedObjects)
        {
            int tagValue = addedObject.tagValue;

            foreach (KeyValuePair<int, ObjectInput> otherObject in SurfaceInputs.Instance.surfaceObjects)
            {
                var nearestDist = 2.0;
                ObjectInput nearestObj = null;
                float distance = Vector3.Distance(addedObject.position, otherObject.Value.position);

                if (addedObject.tagValue != otherObject.Value.tagValue)
                {
                    if (distance < nearestDist)
                    {
                        Debug.Log("Distance based effect on AddedObject");
                        nearestDist = distance;
                        nearestObj = otherObject.Value;
        
                        GameObject instance = Instantiate(expoldingPrefab, GetCenter(addedObject.position, nearestObj.position), Quaternion.identity);
                        distanceInstances.Add(tagValue, instance);
                       
                    }
                }
                else
                    continue;
            }
        }
    }
    void UpdateObjects(List<ObjectInput> updatedObjects)
    {
        foreach (ObjectInput updatedObject in updatedObjects)
        {
            GameObject instance;
            if (distanceInstances.TryGetValue(updatedObject.tagValue, out instance))
            {
                instance.transform.localPosition = updatedObject.position;
                Debug.Log("Distance based effect on UpdateObject");
                //instance.transform.localRotation = Quaternion.Euler(0, 0, -entry.Value.orientation * Mathf.Rad2Deg);
            }
        }
    }
    void RemoveObjects(List<ObjectInput> removedObjects)
    {
        foreach (ObjectInput removedObject in removedObjects)
        {
            GameObject instance;
            if (distanceInstances.TryGetValue(removedObject.tagValue, out instance))
            {
                Destroy(instance);
                distanceInstances.Remove(removedObject.tagValue);
                Debug.Log("Distance based effect on RemovedObject");
            }
        }
    }

    private Vector3 GetCenter(Vector3 a, Vector3 b)
    {
        return (a + b) / 2;
    }
 }
