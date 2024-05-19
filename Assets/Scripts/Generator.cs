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
        _obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _obj.transform.position = Vector3.zero;
        _obj.transform.localScale = Vector3.one * 5;
        _mesh = _obj.GetComponent<MeshFilter>().mesh;

        var verts = _mesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var vpos = verts[i];
            obj.transform.position = new Vector3(vpos.x * 5
                , vpos.y * 5
                , vpos.z * 5);
            obj.transform.localScale = Vector3.one * .1f;
            obj.name = $"vert:{i}";
        }

        var triangles = _mesh.GetTriangles(0);
        foreach (var i in triangles)
        {
            Debug.Log($"{i}");
        }

        var ax = 0f;
        var ay = 0f;
        var az = 0f;

        ax = verts.Sum(_ => _.x) / verts.Length;
        ay = verts.Sum(_ => _.y) / verts.Length;
        az = verts.Sum(_ => _.z) / verts.Length;

        var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
        o.transform.position = new Vector3(ax, ay, az);
        o.transform.localScale = Vector3.one * .1f;
    }

    void Update()
    {
    }
}