using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EarthBuilder : MonoBehaviour
{
    [SerializeField]
    private Texture2D image; 

    [SerializeField]
    private float minimumRadius = 1f;

    [SerializeField]
    private float maximumHeightDifferenceMultiplier = .5f;

    [SerializeField]
    private int verticalDivisions = 10;

    [SerializeField]
    private int horizontalDivisions = 10;

    [SerializeField]
    private bool shouldCreateAsset = false;

    [SerializeField]
    private string assetName;

    [SerializeField]
    private DebugManager debugManager;

    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            UpdateMeshes();
            MeshUtility.Optimize(mesh);

            //DebugPoints();

            if(shouldCreateAsset)
            {
                AssetDatabase.CreateAsset(mesh, 
                    $"Assets/{assetName}-{horizontalDivisions}-{verticalDivisions}-{maximumHeightDifferenceMultiplier}.asset");
                AssetDatabase.SaveAssets();
            }
        }
    }

    private void DebugPoints()
    {
        var indexesList = new List<int>();

        indexesList.AddRange(GetNorthPoleMesh());
        indexesList.AddRange(GetLongitudinalMeshes());
        indexesList.AddRange(GetSouthPoleMesh());


        debugManager.DebugPoints(GetModelPoints(), indexesList);
    }

    private void UpdateMeshes()
    {
        var indexesList = new List<int>();

        indexesList.AddRange(GetNorthPoleMesh());
        indexesList.AddRange(GetLongitudinalMeshes());
        indexesList.AddRange(GetSouthPoleMesh());

        mesh.vertices = GetModelPoints().ToArray();
        mesh.triangles = indexesList.ToArray();

    }

    private List<int> GetNorthPoleMesh()
    {
        List<int> polarTriangles = new List<int>(); 

        for(int k = 1; k <= horizontalDivisions; k++)
        {
            if(k < horizontalDivisions)
            {
                polarTriangles.Add(0);
                polarTriangles.Add(k+1);
                polarTriangles.Add(k);
                continue;
            }

            polarTriangles.Add(0);
            polarTriangles.Add(1);
            polarTriangles.Add(k);
            
        }

        return polarTriangles;
    }

    private List<int> GetSouthPoleMesh()
    {
        List<int> polarTriangles = new List<int>(); 

        var numberOfPolarPointsDiscarded = horizontalDivisions - 2;

        var vertexCount = horizontalDivisions * verticalDivisions - numberOfPolarPointsDiscarded - 1;

        for(int k = vertexCount; k >= vertexCount - horizontalDivisions; k--)
        {
            if(k > vertexCount - horizontalDivisions)
            {
                polarTriangles.Add(vertexCount);
                polarTriangles.Add(k-1);
                polarTriangles.Add(k);
                continue;
            }

            polarTriangles.Add(vertexCount);
            polarTriangles.Add(vertexCount - 1);
            polarTriangles.Add(k);
            
        }

        return polarTriangles;
    }
 
    private List<int> GetLongitudinalMeshes()
    {
        List<int> triangles = new List<int>();

        for(int j = 0; j < verticalDivisions - 2; j++)
        {
            var firstOfTheLine = horizontalDivisions * j + 1;

            for(int i = firstOfTheLine; i < horizontalDivisions + firstOfTheLine - 1; i++)
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + horizontalDivisions);

                triangles.Add(i + 1);
                triangles.Add( i + 1 + horizontalDivisions);
                triangles.Add(i + horizontalDivisions);
            }

            triangles.Add(horizontalDivisions + firstOfTheLine - 1);
            triangles.Add(firstOfTheLine);
            triangles.Add(firstOfTheLine + 2 * horizontalDivisions - 1);

            triangles.Add(firstOfTheLine);
            triangles.Add(firstOfTheLine + horizontalDivisions);
            triangles.Add(firstOfTheLine + 2 * horizontalDivisions - 1);
        }

        return triangles;
    }

    private List<Vector3> GetModelPoints()
    {
        var modelPoints = new List<Vector3>();
        
        var pixelsValues = GetRelevantPixelsValues();

        var horizontalAngleStep = 360f / horizontalDivisions;
        var verticalAngleStep = 180f / verticalDivisions;

        var equatorPerimeter = 2 * Mathf.PI * minimumRadius;

        var point = new Vector3();

        for(int j = 0; j <= verticalDivisions; j++)
        {
            for(int i = 0; i < horizontalDivisions; i++)
            {

                float horizontalAngle = horizontalAngleStep * i;
                float verticalAngle = verticalAngleStep * j;

                var horizontalAngleInRad = horizontalAngle * Mathf.Deg2Rad;
                var verticalAngleInRad = verticalAngle * Mathf.Deg2Rad;

                var directionVector = new Vector3(Mathf.Cos(horizontalAngleInRad) * Mathf.Sin(verticalAngleInRad),
                                                            Mathf.Cos(verticalAngleInRad), 
                                                            Mathf.Sin(horizontalAngleInRad) * Mathf.Sin(verticalAngleInRad));
                
                var pointValueIndex = j * verticalDivisions + i;
                var additionalHeight = 0f;
                additionalHeight = maximumHeightDifferenceMultiplier * pixelsValues[pointValueIndex];
                point = directionVector * (minimumRadius + additionalHeight);

                if(j == verticalDivisions || j == 0)
                {
                    modelPoints.Add(point);
                    break;
                }                

                modelPoints.Add(point);
            }
        }
        Debug.Log(modelPoints.Count);
        Debug.Log(pixelsValues.Count);
        return modelPoints;
    }

    private List<float> GetRelevantPixelsValues()
    {
        var horizontalStep = image.width / horizontalDivisions;
        var verticalStep = image.height / verticalDivisions;

        var pixelsValues = new List<float>();

        for(int j = 0; j <= verticalDivisions; j++)
        {
            //for(int i = 0; i < horizontalDivisions; i++)
            for(int i = horizontalDivisions - 1; i >= 0; i--)
            {
                Color pixel = new Color();

                pixel = image.GetPixel(i * horizontalStep, j * verticalStep);
                pixelsValues.Add(pixel.grayscale);
            }
        }

        pixelsValues.Reverse();

        return pixelsValues;
    }
}
