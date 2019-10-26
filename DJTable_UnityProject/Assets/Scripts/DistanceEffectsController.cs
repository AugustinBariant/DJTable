using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEffectsController : MonoBehaviour
{
    public GameObject expoldingPrefab;


    void Start()
    {
        SurfaceInputs.Instance.OnObjectAdd += DistanceEffect;
    }

    void DistanceEffect(List<ObjectInput> addedObjects)
    {
        foreach (ObjectInput addedObject in addedObjects)
        {
            foreach (KeyValuePair<int, ObjectInput> otherObject in SurfaceInputs.Instance.surfaceObjects)
            {
                var nearestDist = 2.0;
                ObjectInput nearestObj = null;
                float distance = Vector3.Distance(addedObject.position, otherObject.Value.position);

                if (addedObject.tagValue != otherObject.Value.tagValue)
                {
                    if (distance < nearestDist)
                    {
                        nearestDist = distance;
                        nearestObj = otherObject.Value;
                        Debug.Log("Distance based effect");
                        GameObject instance = Instantiate(expoldingPrefab, GetCenter(addedObject.position, nearestObj.position), Quaternion.identity);
                        //distEffectInstances.Add(tagValue, instance);
                    }
                    //Debug.Log("okay");
                    //Debug.DrawLine(addedObject.position, nearestObj.position, Color.red);
                }
                else
                    continue;
            }
        }
    }

    private Vector3 GetCenter(Vector3 a, Vector3 b)
    {
        return (a + b) / 2;
    }
 }
