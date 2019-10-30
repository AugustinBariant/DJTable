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
        groupList = new List<ObjectGroup>();
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
                    group.groupCenter = center;
                    Debug.Log("Adding to existing group");
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
                    Debug.Log("Adding to a single object");
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
           //for each group that has been updated
            foreach (ObjectGroup group in groupList)
            {
                if (group.objectList.Contains(updatedObject))
                {
                    Debug.Log("Found group!");
                    //check the distance between the new object and the existing groups center
                    float distance = Vector3.Distance(updatedObject.position, group.groupCenter);
                    if (distance > nearestDist)
                    {
                        Debug.Log("Leaving group!");
                        // Distance too big, so we remove the object from the group
                        group.removeObject(updatedObject);
                        if (group.objectList.Count < 2)
                        {
                            Debug.Log("Group too small!");
                            Destroy(group.effectInstance);
                            foreach (ObjectInput obj in group.objectList)
                            {
                                singleObjects.Add(obj);
                            }
                            groupList.Remove(group);
                        }

                        grouped = true;
                        break;
                    }
                    Vector2 center = GetCenter(group);
                    group.groupCenter = center;
                    break;
                }
            }

            if (grouped == true)
            {
                continue;
            }

            foreach (ObjectGroup otherGroup in groupList)
            {
                //check the distance between the new object and the existing groups center
                float distance = Vector3.Distance(updatedObject.position, otherGroup.groupCenter);
                if (distance < nearestDist)
                {
                    // add addedObject to group's object list
                    otherGroup.addObject(updatedObject);
                    // recalculate and update group center point
                    Vector2 center = GetCenter(otherGroup);
                    // update position and size of the effect instance
                    otherGroup.groupCenter = center;
                    Debug.Log("updating to other group -updated");

                    grouped = true;
                    break;
                }
            }

            if (grouped == true)
            {
                continue;
            }

            foreach (ObjectInput otherObject in singleObjects)
            {
                if (updatedObject.id == otherObject.id)
                {
                    continue;
                }
                float distance = Vector3.Distance(updatedObject.position, otherObject.position);
                if (distance < nearestDist)
                {
                   
                    Vector2 center = (updatedObject.position + otherObject.position) / 2;
                    GameObject instance = Instantiate(expoldingPrefab, center, Quaternion.identity);

                    Debug.Log("Creating new group");
                    List<ObjectInput> objects = new List<ObjectInput>();
                    objects.Add(updatedObject);
                    objects.Add(otherObject);
                    ObjectGroup objectGroup = new ObjectGroup(instance, objects, center);
                    groupList.Add(objectGroup);

                    singleObjects.Remove(otherObject);
                    singleObjects.Remove(updatedObject);
                    Debug.Log("updating to single -updated");

                    grouped = true;
                    break;
                }
            }

            if (grouped == true)
            {
                continue;
            }

            singleObjects.Add(updatedObject);
        }
     }
    void RemoveObjects(List<ObjectInput> removedObjects)
    {
        foreach (ObjectInput removedObject in removedObjects)
        {
            foreach (ObjectGroup group in groupList)
            {
                if (group.objectList.Contains(removedObject))
                {
                    if (group.objectList.Count < 2)
                    {
                        Debug.Log("My friend is removed");
                        Destroy(group.effectInstance);
                        foreach (ObjectInput obj in group.objectList)
                        {
                            singleObjects.Add(obj);
                        }
                        groupList.Remove(group);
                        grouped = false;
                    }
                    else
                    {
                        group.removeObject(removedObject);
                        // recalculate and update group center point
                        Vector2 center = GetCenter(group);
                        // update position and size of the effect instance
                        group.groupCenter = center;
                    }
                }
            }
            singleObjects.Add(removedObject);
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
    //private Vector2 Scale(List<ObjectGroup> groups)
    //{
    //    foreach(ObjectGroup )
    //}
} 

class ObjectGroup
{
    public GameObject effectInstance;

    public Vector2 groupCenter;

    public List<ObjectInput> objectList;
    private Dictionary<ObjectInput, GameObject> distanceInstances;

    //Constructor
    public ObjectGroup(GameObject effectInstance, List<ObjectInput> objectList, Vector2 center)
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
