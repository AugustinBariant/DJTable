using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    private LineRenderer lineRenderer;

    //Starting and ending point
    public Transform start;
    public Transform end;

    private readonly int pointsCount = 5;
    private Vector3[] points;

    private readonly int pointIndexA = 0;
    private readonly int pointIndexB = 1;
    private readonly int pointIndexC = 2;
    private readonly int pointIndexD = 3;
    private readonly int pointIndexE = 4;

    private float distance;
    private float timer;
    private float timerTimeOut = 0.05f;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        points = new Vector3[pointsCount];
        lineRenderer.positionCount = pointsCount;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > timerTimeOut)
        {
            timer = 0;

            points[pointIndexA] = start.position;
            points[pointIndexE] = end.position;
            points[pointIndexC] = GetCenter(points[pointIndexA], points[pointIndexE]);
            points[pointIndexB] = GetCenter(points[pointIndexA], points[pointIndexC]);
            points[pointIndexD] = GetCenter(points[pointIndexC], points[pointIndexE]);

            distance = Vector3.Distance(start.position, end.position) / points.Length;

            if (distance < 2)
            {
                lineRenderer.SetPositions(points);
                lineRenderer.SetVertexCount(5);
                Debug.Log("THE LINE!!!!!");
            }else
            {
                //Destroy(lineRenderer.gameObject, 5);
                lineRenderer.SetVertexCount(0);
                Debug.Log("DESTROYED");

            }
        }
    }

    private Vector3 GetCenter(Vector3 a, Vector3 b)
    {
        return (a + b) / 2;
    }
}


