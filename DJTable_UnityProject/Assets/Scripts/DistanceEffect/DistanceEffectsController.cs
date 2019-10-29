using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEffectsController : MonoBehaviour
{
    public GameObject expoldingPrefab;

    private List<ObjectInput> singleObjects;
    private List<ObjectGroup> groupList;

    private bool grouped = false;
    const float nearestDist = 3f;

    void Start()
    {

        SurfaceInputs.Instance.OnObjectAdd += AddNewObjects;
        SurfaceInputs.Instance.OnObjectUpdate += UpdateObjects;
        SurfaceInputs.Instance.OnObjectRemove += RemoveObjects;

        singleObjects = new List<ObjectInput>(SurfaceInputs.Instance.surfaceObjects.Values);
    }

    void AddNewObjects(List<ObjectInput> addedObjects)
    {
        foreach (ObjectInput addedObject in addedObjects)
        {
            //go through each group in the group list
            foreach (ObjectGroup group in groupList)
            {
                //check the distance between the new object and the existing groups center
                float distance = Vector3.Distance(addedObject.position, group.groupCenter);
                if (distance < nearestDist)
                {
                    // add addedObject to group's object list
                    group.addObject(addedObject);
                    // recalculate and update group center point
                    Vector2 center = GetCenter(group);
                    // update position and size of the effect instance
                    GameObject instance = Instantiate(expoldingPrefab, center, Quaternion.identity);

                    //ObjectGroup objectGroup = new ObjectGroup(instance, objects, center);
                    //groupList.Add(objectGroup);

                    grouped = true;
                    break;
                }
            }

            if (grouped == true)
            {
                continue;
            }
            //Instantiate an effect on a group of 2 single objects
            foreach (ObjectInput otherObject in singleObjects)
            {
                float distance = Vector3.Distance(addedObject.position, otherObject.position);

                if (addedObject.tagValue != otherObject.tagValue)
                {
                    if (distance < nearestDist)
                    {
                        Vector2 center = (addedObject.position + otherObject.position) / 2;
                        GameObject instance = Instantiate(expoldingPrefab, center, Quaternion.identity);

                        List<ObjectInput> objects = new List<ObjectInput>();
                        objects.Add(addedObject);
                        objects.Add(otherObject);
                        ObjectGroup objectGroup = new ObjectGroup(instance, objects, center);
                        groupList.Add(objectGroup);

                        singleObjects.Remove(otherObject);

                        grouped = true;
                        break;
                    }
                }
            }

            if (grouped == true)
            {
                continue;
            }
            //add to the single object list if none of the statement matches
            singleObjects.Add(addedObject);
        }
    }

    void UpdateObjects(List<ObjectInput> updatedObjects)
    {
        foreach (ObjectInput updatedObject in updatedObjects)
        {
            //    instance.transform.localPosition = updatedObject.position;

        }
    }
    void RemoveObjects(List<ObjectInput> removedObjects)
    {
        foreach (ObjectInput removedObject in removedObjects)
        {
            GameObject instance;
            //if (distanceInstances.TryGetValue(removedObject.tagValue, out instance))
            //{
            //    Destroy(instance);
            //    distanceInstances.Remove(removedObject.tagValue);
            //    Debug.Log("Distance based effect on RemovedObject");
            //}
        }
    }

    private Vector2 GetCenter(ObjectGroup objectGroup)
    {
        var sumX = 0f;
        var sumY = 0f;

        foreach (ObjectInput objectInput in objectGroup.objectList)
        {
            sumX += objectInput.position.x;
            sumY += objectInput.position.y;
        }
        var centerX = sumX / objectGroup.objectList.Count;
        var centerY = sumY / objectGroup.objectList.Count;
        Vector2 center = new Vector2(centerX, centerY);
        return center;
    } 

    //ObjectGroup checkGroup(ObjectInput addedObject, ObjectInput otherObject)
    //{

    //    //for each group in the group list
    //    foreach (ObjectGroup currentGroup in groupList)
    //    {
    //        //for each object in the group
    //        foreach (ObjectInput currentObject in currentGroup.objectList)
    //        {
    //            //if other object exists
    //            if(otherObject.tagValue == currentObject.tagValue)
    //            {
    //                //add the new object to the group
    //                currentGroup.addObject(addedObject);

    //                return currentGroup;
    //            }
    //        }
    //    }
    //    //otherwise create a new group for them
    //    ObjectGroup newGroup = new ObjectGroup();
    //    newGroup.addObject(addedObject);
    //    newGroup.addObject(otherObject);

    //    groupList.Add(newGroup);

    //    return newGroup;

    //}


}

class ObjectGroup
{
    GameObject effectInstance;
    public List<ObjectInput> objectList;
    public Vector2 groupCenter;
    private Dictionary<ObjectInput, GameObject> distanceInstances;

    //Constructor
    public ObjectGroup(GameObject effectInstane, List<ObjectInput> objectList, Vector2 center)
    {
        this.effectInstance = effectInstance;
        this.objectList = objectList;
        this.groupCenter = center;
    }

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
