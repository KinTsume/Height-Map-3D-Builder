using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{

    [SerializeField]
    private GameObject pointSphere;

    [SerializeField]
    private float pointSize;

    [SerializeField]
    private Queue<Vector3> pointsToDebug = new Queue<Vector3>();

    [SerializeField]
    private Queue<Vector3> meshesToDebug = new Queue<Vector3>();

    [SerializeField]
    private float timeInterval;

    [SerializeField]
    private LineRenderer lineRenderer;

    private int index = 0;

    private bool startPointDebugging = false;
    private float time = 0;
    private bool startDrawingMeshes = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(time > timeInterval && startPointDebugging)
        {
            time = 0;
            var choosenPoint = pointsToDebug.Dequeue();
            InstantiatePoint(choosenPoint);

            if(pointsToDebug.Count < 1)
            {
                startDrawingMeshes = true;
                startPointDebugging = false;
            }
        }else if(time > timeInterval && startDrawingMeshes)
        {

            time = 0;
            var choosenPoint = meshesToDebug.Dequeue();
            DrawLine(choosenPoint);
        }

        time += Time.deltaTime;
    }

    private void DrawLine(Vector3 choosenPoint)
    {
        var thirdVector = lineRenderer.GetPosition(2);
        if(thirdVector != Vector3.zero)
        {
            lineRenderer.positionCount = 3;
            index = 0;

            for(int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, Vector3.zero);
            }
        }

        lineRenderer.SetPosition(index, choosenPoint);
        index++;
    }

    private void InstantiatePoint(Vector3 point)
    {
        var instantiatedSphere = Instantiate(pointSphere, point, Quaternion.identity);
        instantiatedSphere.transform.localScale = new Vector3(pointSize, pointSize, pointSize);
    }

    public void DebugPoints(List<Vector3> points, List<int> meshesIndexes)
    {
        lineRenderer.positionCount = 3;

        foreach(var point in points)
        {
            pointsToDebug.Enqueue(point);
        }

        foreach(var mesh in meshesIndexes)
        {
            meshesToDebug.Enqueue(points[mesh]);
        }

        startPointDebugging = true;
    }
}
