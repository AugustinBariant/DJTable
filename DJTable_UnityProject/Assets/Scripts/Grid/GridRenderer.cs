using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridRenderer : MonoBehaviour
{

    private Mesh gridMesh;
    private Renderer renderer;
    private const int WIDTH = 49;
    private const int HEIGHT = 28;

    private float lineGap;

    private float absWidth;
    private float absHeight;

    private float timeElapsed = 0f;
    private int wavePos = 0;

    private Vector3[] baseVertices;

    private Dictionary<int, FingerInput> surfaceFingers = new Dictionary<int, FingerInput>();
    private Dictionary<int, ObjectInput> surfaceObjects = new Dictionary<int, ObjectInput>();

    // Start is called before the first frame update
    void Start()
    {
        Vector3 screenSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
        lineGap = screenSize.x / (WIDTH - 1);
        absWidth = screenSize.x;
        absHeight = screenSize.y;

        SurfaceInputs.Instance.OnTouch += ProcessObjects;

        renderer = GetComponent<MeshRenderer>();
        renderer.material.SetFloat("_LineGap", lineGap);
        //renderer.material.SetVectorArray("_ObjectPositions", new List<Vector4>());
        renderer.material.SetInt("_NumObjects", 0);

        GenerateMesh();
        transform.Translate(new Vector3(0, 0, 5));
    }

    void ProcessObjects(Dictionary<int, FingerInput> surfaceFingers, Dictionary<int, ObjectInput> surfaceObjects)
    {
        this.surfaceFingers = surfaceFingers;
        this.surfaceObjects = surfaceObjects;
    }

    private void GenerateMesh()
    {
        gridMesh = new Mesh();
        gridMesh.name = "Cool Procedural Grid ayyyoo";

        Vector3[] vertices = new Vector3[WIDTH * HEIGHT];
        Vector2[] uv = new Vector2[vertices.Length];

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                int idx = (y * WIDTH) + x;

                vertices[idx] = new Vector3(x * lineGap, y * lineGap, 0);
                uv[idx] = new Vector2((float)x / WIDTH, (float)y / HEIGHT);
            }
        }
        baseVertices = vertices;
        gridMesh.vertices = vertices;
        gridMesh.uv = uv;

        List<int> triangles = new List<int>();
        for (int y = 0; y < HEIGHT - 1; y++)
        {
            for (int x = 1; x < WIDTH; x++)
            {
                int bottomRowX = y * WIDTH;
                int topRowX = bottomRowX + WIDTH;

                triangles.Add(bottomRowX + x - 1);
                triangles.Add(topRowX + x - 1);
                triangles.Add(topRowX + x);

                triangles.Add(bottomRowX + x - 1);
                triangles.Add(topRowX + x);
                triangles.Add(bottomRowX + x);
            }
        }

        gridMesh.triangles = triangles.ToArray();
        gridMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = gridMesh;
    }
    private int GetVertexForCoords(int x, int y)
    {
        return (y * WIDTH) + x;
    }

    private int GetVertexForCoords(Vector2 coords)
    {
        return (int)((coords.y * WIDTH) + coords.x);
    }

    private Vector2 RelativeToPixelCoords(Vector3 coords)
    {
        int y = Mathf.Clamp((int)(coords.y * HEIGHT), 0, HEIGHT - 1);
        int x = Mathf.Clamp((int)(coords.x * WIDTH), 0, WIDTH - 1);
        return new Vector2(x, y);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] vertices = (Vector3[])baseVertices.Clone();

        List<Vector4> objectPositions = new List<Vector4>();
        
        foreach (KeyValuePair<int, ObjectInput> obj in surfaceObjects)
        {
            Vector2 absPosition = obj.Value.position;
            objectPositions.Add(absPosition);

            Vector2 coords = RelativeToPixelCoords(obj.Value.posRelative);

            for (int x = (int)coords.x - 2; x <= (int)coords.x + 2; x++)
            {
                for (int y = (int)coords.y - 2; y <= (int)coords.y + 2; y++)
                {
                    int i = GetVertexForCoords(x, y);
                    if (i < 0 || i >= vertices.Length)
                    {
                        continue;
                    }
                    float dist = Vector2.Distance(absPosition, vertices[i]);
                    vertices[i].z -= Mathf.Sqrt(0.5f / dist);
                }
            }

        }
        gridMesh.vertices = vertices;
        gridMesh.RecalculateNormals();

        if (objectPositions.Count > 0)
        {
            Matrix4x4 objectPositions1 = new Matrix4x4();
            Matrix4x4 objectPositions2 = new Matrix4x4();

            for (int i = 0; i < objectPositions.Count; i++) {
                if (i < 4) {
                    objectPositions1.SetRow(i, objectPositions[i]);
                } else {
                    objectPositions2.SetRow(i - 4, objectPositions[i]);
                }
            }
            renderer.material.SetMatrix("_ObjectPositions1", objectPositions1);
            renderer.material.SetMatrix("_ObjectPositions2", objectPositions2);
            // renderer.material.SetVectorArray("_ObjectPositions", objectPositions);
        }
        renderer.material.SetInt("_NumObjects", objectPositions.Count);
    }
}
