using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisualEffectsController : MonoBehaviour
{
    [Header("Particle prefabs")]
    public GameObject[] prefabs = new GameObject[8];

    public Color[] dialColors = new Color[8];
    public GameObject dialPrefab;
    public GameObject explosionPrefab;

    private Dictionary<int, GameObject> objectPrefabs;
    private Dictionary<int, GameObject> effectInstances;
    private Dictionary<int, GameObject> dialInstances;

    private const float halfPI = Mathf.PI / 2f;

    private LineRenderer lineRenderer;

    private readonly int pointsCount = 5;
    private Vector3[] points;

    private readonly int pointIndexA = 0;
    private readonly int pointIndexB = 1;
    private readonly int pointIndexC = 2;
    private readonly int pointIndexD = 3;
    private readonly int pointIndexE = 4;

    private float timer;
    private float timerTimeOut = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        objectPrefabs = new Dictionary<int, GameObject>();
        effectInstances = new Dictionary<int, GameObject>();
        dialInstances = new Dictionary<int, GameObject>();

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

        lineRenderer = GetComponent<LineRenderer>();
        points = new Vector3[pointsCount];
        lineRenderer.positionCount = pointsCount;
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

            if (!dialInstances.ContainsKey(tagValue))
            {
                GameObject instance = Instantiate(dialPrefab, addedObject.position, Quaternion.identity);
                ParticleSystem[] particles = instance.GetComponentsInChildren<ParticleSystem>();

                Color color = dialColors[tagValue];
                color.a = 0.36f;

                foreach (ParticleSystem p in particles)
                {
                    ParticleSystem.MainModule main = p.main;
                    main.startColor = color;
                }

                Transform pointerTransform = instance.transform.GetChild(0);
                pointerTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * addedObject.orientation);

                Transform indicatorTransform = instance.transform.GetChild(1);
                indicatorTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * halfPI * (int)(addedObject.orientation / halfPI));

                dialInstances.Add(tagValue, instance);
            }

            //Adds the explosion prefab
            if (Vector3.Distance(transform.position, addedObject.position) < 10)
            {
                GameObject instance = Instantiate(explosionPrefab, addedObject.position, Quaternion.identity);
                effectInstances.Add(tagValue, instance);
            }



            //timer += Time.deltaTime;

            //if (timer > timerTimeOut)
            //{
            //    timer = 0;

            //    points[pointIndexA] = transform.position;
            //    points[pointIndexE] = addedObject.position;
            //    points[pointIndexC] = GetCenter(points[pointIndexA], points[pointIndexE]);
            //    points[pointIndexB] = GetCenter(points[pointIndexA], points[pointIndexC]);
            //    points[pointIndexD] = GetCenter(points[pointIndexC], points[pointIndexE]);

            //    //distance = Vector3.Distance(start.position, end.position) / points.Length;

            //    if ((Vector3.Distance(transform.position, addedObject.position) / points.Length) < 2)
            //    {
            //        lineRenderer.SetPositions(points);
            //        lineRenderer.SetVertexCount(5);
            //        Debug.Log("THE LINE!!!!!");
            //    }
            //    else
            //    {
            //        //Destroy(lineRenderer.gameObject, 5);
            //        lineRenderer.SetVertexCount(0);
            //        Debug.Log("DESTROYED");

            //    }
            //}
        }
    }
    private Vector3 GetCenter(Vector3 a, Vector3 b)
    {
        return (a + b) / 2;
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

            if (dialInstances.TryGetValue(updatedObject.tagValue, out instance))
            {
                instance.transform.localPosition = updatedObject.position;
                Transform pointerTransform = instance.transform.GetChild(0);
                pointerTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * updatedObject.orientation);

                Transform indicatorTransform = instance.transform.GetChild(1);
                indicatorTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * halfPI * (int)(updatedObject.orientation / halfPI));
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

            if (dialInstances.TryGetValue(removedObject.tagValue, out instance))
            {
                Destroy(instance);
                dialInstances.Remove(removedObject.tagValue);
            }
        }
    }

}

