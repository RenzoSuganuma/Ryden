using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshModifierHelper : MonoBehaviour
{
    [SerializeField] private GameObject dividedTarget; // the target to divide
    [SerializeField] private Vector2 divisionCount; // the division count x-y axis

    void Start()
    {
        var mesh = dividedTarget.GetComponent<MeshFilter>()
            .mesh;

        List<Vector3> verts = new List<Vector3>();
        mesh.GetVertices(verts);
        
        Debug.Log($"{FindMiddleXAxis(verts)} Count:{verts.Count}");
    }

    // 分割数に応じてメッシュをカットする
    private void CutMesh(Mesh source, out Mesh dividedMesh, int divisionX, int divisionY)
    {
        dividedMesh = source;
    }

    // 辺を取得する
    private void GetEdges(List<int> edges, Mesh mesh)
    {
        var triangles = mesh.GetTriangles(0);
        edges.Clear();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            edges.Add(i);
            edges.Add(i + 1);

            edges.Add(i + 1);
            edges.Add(i + 2);

            edges.Add(i + 2);
            edges.Add(i);
        }
    }

    // ｙ軸の中点 頂点の中で最も高い位置と最も低い位置のデータを参照している
    private float FindMiddleYAxis(List<Vector3> verts)
    {
        float max, min;
        // テキトーに初期化
        max = verts[verts.Count - 1].y;
        min = verts[0].y;

        for (int i = 0; i < verts.Count; i++)
        {
            // 最大値
            if (verts[i].y > max)
            {
                max = verts[i].y;
            }

            // 最小値
            if (verts[i].y < min)
            {
                min = verts[i].y;
            }
        }

        var v = (min - max) / 2f; // 最高点から中点へのベクトル = 最高点から最低点へのベクトル / 2
        return max + v;
    }

    // ｘ軸の中点 頂点の中で最も高い位置と最も低い位置のデータを参照している
    private float FindMiddleXAxis(List<Vector3> verts)
    {
        float max, min;
        // テキトーに初期化
        max = verts[verts.Count - 1].x;
        min = verts[0].x;

        for (int i = 0; i < verts.Count; i++)
        {
            // 最大値
            if (verts[i].x > max)
            {
                max = verts[i].x;
            }

            // 最小値
            if (verts[i].x < min)
            {
                min = verts[i].x;
            }
        }

        var v = (min - max) / 2f; // 最高点から中点へのベクトル = 最高点から最低点へのベクトル / 2
        return max + v;
    }

    private void MakePyramid()
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

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
        obj.GetComponent<MeshFilter>().mesh = nMesh;
    }

    void Update()
    {
    }
}