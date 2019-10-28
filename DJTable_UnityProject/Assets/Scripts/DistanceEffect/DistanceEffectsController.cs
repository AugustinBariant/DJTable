using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEffectsController : MonoBehaviour
{
    public GameObject expoldingPrefab;
    private Dictionary<int, GameObject> distanceInstances;
    private Dictionary<ObjectGroup, GameObject> effectInstances;

    private List<ObjectGroup> groupList;

    void Start()
    {
        distanceInstances = new Dictionary<int, GameObject>();
        effectInstances = new Dictionary<ObjectGroup, GameObject>();

        SurfaceInputs.Instance.OnObjectAdd += AddNewObjects;
        SurfaceInputs.Instance.OnObjectUpdate += UpdateObjects;
        SurfaceInputs.Instance.OnObjectRemove += RemoveObjects;
    }

    void AddNewObjects(List<ObjectInput> addedObjects)
    {
        List<ObjectInput> otherObjects = new List<ObjectInput>(SurfaceInputs.Instance.surfaceObjects.Values);
        
  
        foreach (ObjectInput addedObject in addedObjects)
        {
            foreach (ObjectInput otherObject in otherObjects)
            {
                var nearestDist = 2.0;
                float distance = Vector3.Distance(addedObject.position, otherObject.position);

                if (addedObject.tagValue != otherObject.tagValue)
                {
                    if (distance < nearestDist)
                    {
                        //Debug.Log("Distance based effect on AddedObject");
                        ObjectGroup currentGroup = checkGroup(addedObject, otherObject);
                        GameObject instance = Instantiate(expoldingPrefab, GetCenter(currentGroup), Quaternion.identity);
                        Debug.Log("Distance based effect on AddedObject");

                        //GameObject instance = Instantiate(expoldingPrefab, GetCenter(addedObject.position, otherObject.position), Quaternion.identity);
                        Debug.Log("Object:" + addedObject.id + addedObject.position);

                        //distanceInstances.Add(addedObject.tagValue, instance);
                        effectInstances.Add(currentGroup, instance);
                    }
                    else
                        Destroy(gameObject);
                }
            }
        }
    }

    ObjectGroup checkGroup(ObjectInput addedObject, ObjectInput otherObject)
    {
        
        //for each group in the group list
        foreach (ObjectGroup currentGroup in groupList)
        {
            //for each object in the group
            foreach (ObjectInput currentObject in currentGroup.objectList)
            {
                //if other object exists
                if(otherObject.id == currentObject.id)
                {
                    //add the new object to the group
                    currentGroup.addObject(addedObject);

                    return currentGroup;
                }
            }
        }
        //otherwise create a new group for them
        ObjectGroup newGroup = new ObjectGroup();
        newGroup.addObject(addedObject);
        newGroup.addObject(otherObject);
        groupList.Add(newGroup);
   
        return newGroup;

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



    private Vector3 GetCenter(ObjectGroup objectGroup)
    {
        float sumx = 0f;
        float sumy = 0f;

        foreach (ObjectInput objectInput in objectGroup.objectList)
        {
            sumx += objectInput.position.x;
            sumy += objectInput.position.y;
        }
        var centerx = sumx / objectGroup.objectList.Count;
        var centery = sumy / objectGroup.objectList.Count;
        Vector2 center = new Vector2(centerx, centery);
        return center;
    }

    //private Vector3 GetCenter(Vector3 a, Vector3 b)
    //{
    //    return (a + b) / 2;

    //}

}

class ObjectGroup
{
    GameObject effectInstance;
    public List<ObjectInput> objectList;

    //Add an object into the list
    public void addObject(ObjectInput id)
    {
        objectList.Add(id);
    }
    //Remove an object from the list
    public void removeObject(ObjectInput id)
    {
        objectList.Remove(id);
    }

}
