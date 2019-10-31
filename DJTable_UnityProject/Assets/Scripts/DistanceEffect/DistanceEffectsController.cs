using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEffectsController : MonoBehaviour
{
    private static DistanceEffectsController _instance;
    public static DistanceEffectsController Instance { get { return _instance; } }

    public GameObject expoldingPrefab;

    private List<ObjectInput> singleObjects;
    private List<ObjectGroup> groupList;

    // Bool flags for fast checks to see if an object is grouped
    public bool[] objectsGrouped { get; private set; }

    // Event used to notify other controllers that some objects have become (un)grouped
    public delegate void GroupingStatusHandler(List<ObjectInput> objects);
    public event GroupingStatusHandler OnGroupingChange;

    const float SINGLE_DIST = 2f;
    const float GROUP_DIST = 1f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        SurfaceInputs.Instance.OnObjectAdd += AddNewObjects;
        SurfaceInputs.Instance.OnObjectUpdate += UpdateObjects;
        SurfaceInputs.Instance.OnObjectRemove += RemoveObjects;

        singleObjects = new List<ObjectInput>(SurfaceInputs.Instance.surfaceObjects.Values);
        groupList = new List<ObjectGroup>();
        objectsGrouped = new bool[8] { false, false, false, false, false, false, false, false };

    }

    void AddNewObjects(List<ObjectInput> addedObjects)
    {
        List<ObjectInput> changedObjects = new List<ObjectInput>();

        foreach (ObjectInput addedObject in addedObjects)
        {
            bool grouped = false; 

            //go through each group in the group list
            foreach (ObjectGroup group in groupList)
            {
                //check the distance between the new object and the existing groups center
                float distance = Vector2.Distance(addedObject.position, group.groupCenter);
                if (distance < GROUP_DIST)
                {
                    // add addedObject to group's object list
                    group.addObject(addedObject);
                    // recalculate and update group center point
                    Vector2 center = GetCenter(group);
                    // update position and size of the effect instance
                    group.groupCenter = center;
                    grouped = true;

                    objectsGrouped[addedObject.tagValue] = true;
                    changedObjects.Add(addedObject);
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
                if (addedObject.id == otherObject.id)
                {
                    continue;
                }

                float distance = Vector2.Distance(addedObject.position, otherObject.position);
                if (distance < SINGLE_DIST)
                {
                    Vector2 center = (addedObject.position + otherObject.position) / 2;
                    GameObject instance = Instantiate(expoldingPrefab, center, Quaternion.identity);

                    List<ObjectInput> objects = new List<ObjectInput>();
                    objects.Add(addedObject);
                    objects.Add(otherObject);
                    ObjectGroup objectGroup = new ObjectGroup(instance, objects, center);
                    groupList.Add(objectGroup);

                    singleObjects.Remove(otherObject);

                    objectsGrouped[addedObject.tagValue] = true;
                    objectsGrouped[otherObject.tagValue] = true;
                    changedObjects.Add(addedObject);
                    changedObjects.Add(otherObject);

                    grouped = true;
                    break;
                }
            }

            if (grouped == true)
            {
                continue;
            }
            //add to the single object list if none of the statement matches
            singleObjects.Add(addedObject);
        }

        if (OnGroupingChange != null && changedObjects.Count > 0)
        {
            OnGroupingChange(changedObjects);
        }
    }

    void UpdateObjects(List<ObjectInput> updatedObjects)
    {
        List<ObjectInput> changedObjects = new List<ObjectInput>();

        foreach (ObjectInput updatedObject in updatedObjects)
        {
            bool grouped = false;
           //for each group that has been updated
            foreach (ObjectGroup group in groupList)
            {
                if (group.objectList.Contains(updatedObject))
                {
                    grouped = true;
                    //check the distance between the new object and the existing groups center
                    float distance = Vector2.Distance(updatedObject.position, group.groupCenter);
                    if (distance > GROUP_DIST)
                    {
                        // Distance too big, so we remove the object from the group
                        group.removeObject(updatedObject);
                        objectsGrouped[updatedObject.tagValue] = false;
                        changedObjects.Add(updatedObject);

                        if (group.objectList.Count < 2)
                        {
                            DestroyEffectInstance(group.effectInstance);
                            foreach (ObjectInput obj in group.objectList)
                            {
                                singleObjects.Add(obj);
                                objectsGrouped[obj.tagValue] = false;
                                changedObjects.Add(obj);
                            }
                            groupList.Remove(group);
                            grouped = false;
                            break;
                        }
                        grouped = false;
                    }
                    Vector2 center = GetCenter(group);
                    group.groupCenter = center;
                    group.effectInstance.transform.position = center;
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
                float distance = Vector2.Distance(updatedObject.position, otherGroup.groupCenter);
                if (distance < GROUP_DIST)
                {
                    // add addedObject to group's object list
                    otherGroup.addObject(updatedObject);
                    // recalculate and update group center point
                    Vector2 center = GetCenter(otherGroup);
                    // update position and size of the effect instance
                    otherGroup.groupCenter = center;
                    otherGroup.effectInstance.transform.position = center;

                    grouped = true;
                    changedObjects.Add(updatedObject);
                    objectsGrouped[updatedObject.tagValue] = true;
                    singleObjects.Remove(updatedObject);
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
                float distance = Vector2.Distance(updatedObject.position, otherObject.position);
                if (distance < SINGLE_DIST)
                {
                    Vector2 center = (updatedObject.position + otherObject.position) / 2f;
                    GameObject instance = Instantiate(expoldingPrefab, center, Quaternion.identity);

                    List<ObjectInput> objects = new List<ObjectInput>();
                    objects.Add(updatedObject);
                    objects.Add(otherObject);
                    ObjectGroup objectGroup = new ObjectGroup(instance, objects, center);
                    groupList.Add(objectGroup);

                    singleObjects.Remove(otherObject);
                    singleObjects.Remove(updatedObject);

                    changedObjects.Add(updatedObject);
                    changedObjects.Add(otherObject);
                    objectsGrouped[updatedObject.tagValue] = true;
                    objectsGrouped[otherObject.tagValue] = true;

                    grouped = true;
                    break;
                }
            }

            if (grouped == true)
            {
                continue;
            }

            if (!singleObjects.Contains(updatedObject))
            {
                singleObjects.Add(updatedObject);
            }
        }

        if (OnGroupingChange != null && changedObjects.Count > 0)
        {
            //Debug.Log("GROUPING CHANGED FOR " + changedObjects.Count + " OBJECTS");
            OnGroupingChange(changedObjects);
        }
    }

    void RemoveObjects(List<ObjectInput> removedObjects)
    {
        List<ObjectInput> changedObjects = new List<ObjectInput>();
        
        foreach (ObjectInput removedObject in removedObjects)
        {
            foreach (ObjectGroup group in groupList)
            {
                if (group.objectList.Contains(removedObject))
                {
                    changedObjects.Add(removedObject);
                    group.removeObject(removedObject);

                    if (group.objectList.Count < 2)
                    {
                        DestroyEffectInstance(group.effectInstance);
                        foreach (ObjectInput obj in group.objectList)
                        {
                            singleObjects.Add(obj);
                            objectsGrouped[obj.tagValue] = false;
                            changedObjects.Add(obj);
                        }
                        groupList.Remove(group);
                        break;
                    }

                    Vector2 center = GetCenter(group);
                    group.groupCenter = center;
                    group.effectInstance.transform.position = center;
                    break;
                }
            }
            objectsGrouped[removedObject.tagValue] = false;
            singleObjects.Remove(removedObject);
        }

        if (OnGroupingChange != null && changedObjects.Count > 0)
        {
            //Debug.Log("GROUPING CHANGED FOR " + changedObjects.Count + " OBJECTS");
            OnGroupingChange(changedObjects);
        }
    }

    private Vector2 GetCenter(ObjectGroup objectGroup)
    {
        float maxX = -99999f;
        float minX = 99999f;
        float maxY = -99999f;
        float minY = 99999f;

        foreach (ObjectInput objectInput in objectGroup.objectList)
        {

            if (objectInput.position.x > maxX)
            {
                maxX = objectInput.position.x;
            }
            if (objectInput.position.x < minX)
            {
                minX = objectInput.position.x;
            }    
            
            if (objectInput.position.y > maxY)
            {
                maxY = objectInput.position.y;
            }
            if (objectInput.position.y < minY)
            {
                minY = objectInput.position.y;
            }
        }

        float centerX = (maxX + minX) / 2f;
        float centerY = (maxY + minY) / 2f;
        Vector2 center = new Vector2(centerX, centerY);

        return center;
    }

    private void DestroyEffectInstance(GameObject effectInstance)
    {
        GameObject explodingCircle = effectInstance.transform.GetChild(0).gameObject;
        explodingCircle.GetComponent<ParticleSystem>().Stop();

        GameObject electricBeam = explodingCircle.transform.GetChild(0).gameObject;
        electricBeam.GetComponent<ParticleSystem>().Stop();

        GameObject circle = explodingCircle.transform.GetChild(1).gameObject;
        circle.GetComponent<ParticleSystem>().Stop();

        Destroy(effectInstance, 2);
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
