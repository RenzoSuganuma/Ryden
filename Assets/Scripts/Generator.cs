using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
    private GameObject _obj;
    private Mesh _mesh;

    void Start()
    {
        _obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _mesh = _obj.GetComponent<MeshFilter>().mesh;

        Mesh nMesh = new Mesh();
        Vector3[] verts = new[]
        {
            new Vector3(0, 1, 0), // 0 :up
            new Vector3(0, 0, 1), // 1 :forward
            new Vector3(-1, 0, -1), // 2 :left behind
            new Vector3(1, 0, -1) // 3 :right behind
        };
        int[] triangles = new[]
        {
            0, 3, 2, // backward
            0, 1, 3, // right
            0, 2, 1, // left
            1, 2, 3, // below
        };
        
        nMesh.SetVertices(verts);
        nMesh.SetTriangles(triangles, 0);
        nMesh.RecalculateNormals();
        nMesh.RecalculateBounds();
        nMesh.RecalculateTangents();
        _obj.GetComponent<MeshFilter>().mesh = nMesh;
    }

    void Update()
    {
    }
}