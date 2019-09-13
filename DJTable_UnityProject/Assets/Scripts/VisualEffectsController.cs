using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisualEffectsController : MonoBehaviour
{
    private Camera mainCamera;

    private Dictionary<int, GameObject> objectPrefabs;
    private Dictionary<int, GameObject> effectInstances;


    Dictionary<int, FingerInput> surfaceFingers = new Dictionary<int,FingerInput>();
    Dictionary<int, ObjectInput> surfaceObjects = new Dictionary<int,ObjectInput>();

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        SurfaceInputs.Instance.OnTouch += ProcessObjects;

        GameObject prefab1 = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Fire/Fire.prefab", typeof(GameObject));
        GameObject prefab2 = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Ripple/Big-small-big.prefab", typeof(GameObject));

        objectPrefabs = new Dictionary<int, GameObject>();
        effectInstances = new Dictionary<int, GameObject>();

        objectPrefabs.Add(0, prefab1);
        objectPrefabs.Add(1, prefab2);
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
            //Debug.Log("Checking for " + tagValue);

            GameObject prefab;
            if (objectPrefabs.TryGetValue(tagValue, out prefab))
            {
                //Debug.Log("Instantiate!!!");
                Vector2 screenPos = entry.Value.position;
                Vector3 position = new Vector3((float)screenPos.x * Screen.width, Screen.height - (float)screenPos.y * Screen.height, mainCamera.nearClipPlane);
                position = mainCamera.ScreenToWorldPoint(position);
                Debug.Log(position.ToString());

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

            //Debug.Log(Screen.width);
            //Debug.Log(Screen.height);
            //if (!isThereAnObject)
            //{
            //    Vector2 viewportSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            //    Clone = Instantiate(Prefabs, new Vector2(viewportSize.x * entry.Value.position.x, viewportSize.y * entry.Value.position.y), Quaternion.identity);

            //    //Clone = Instantiate(Prefabs, new Vector2(16* entry.Value.position.x, 9* entry.Value.position.y), Quaternion.identity);
            //    isThereAnObject = true;
            //}

            // Delete stuff that isn' t there
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
