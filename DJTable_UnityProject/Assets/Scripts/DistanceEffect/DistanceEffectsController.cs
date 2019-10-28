using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEffectsController : MonoBehaviour
{
    public GameObject expoldingPrefab;
    private Dictionary<int, GameObject> distanceInstances;
    private List<ObjectGroup> groupList;

    void Start()
    {

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
                        Debug.Log("Distance based effect on AddedObject");
                        ObjectGroup currentGroup = checkGroup(addedObject, otherObject);

                        Debug.Log(currentGroup);
                        GameObject instance = Instantiate(expoldingPrefab, GetCenter(currentGroup), Quaternion.identity);
                        //GameObject instance = Instantiate(expoldingPrefab, GetCenter(addedObject.position, otherObject.position), Quaternion.identity);
                        //Debug.Log("Object id:" + addedObject.id);
                        // distanceInstances.Add(addedObject.tagValue, instance);
                    }
                }
            }
        }
    }

    ObjectGroup checkGroup(ObjectInput addedObject, ObjectInput otherObject)
    {
        foreach (ObjectGroup currentGroup in groupList)
        {
            foreach (ObjectInput currentObject in currentGroup.objectList)
            {
                if(otherObject.id == currentObject.id)
                {
                    currentGroup.addObject(addedObject);
                    return currentGroup;
                }
            }
        }

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
        var bounds = Bounds(objectGroup.objectList[0].position, Vector3.zero);
        foreach (ObjectInput objectInput in objectGroup.objectList)
       
            bounds.Encapsulate(objectGroup.objectList[i].position);
            
       
        return bounds.center;


        //var sum = 0f;
        //foreach(ObjectInput objectInput in objectGroup.objectList)
        //{
        //    sum += objectInput.position; 
        //}
        //var center = sum / objectGroup.Count;
        //return center;

    }
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