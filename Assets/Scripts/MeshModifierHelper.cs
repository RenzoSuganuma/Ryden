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

        CutMesh(mesh, out var dmesh, 0, 2, 2);
    }

    // 分割数に応じてメッシュをカットする
    private void CutMesh(Mesh source, out Mesh dividedMesh, int submeshIndex, int divisionX, int divisionY)
    {
        // 上ベクトルを法線とし、z軸正の向きへ向かう平面
        // 横向きの面
        Plane planeX = new Plane(Vector3.up, Vector3.forward);

        // 右ベクトルを法線とし、z軸正の向きへ向かう平面
        // 縦向きの面
        Plane planeY = new Plane(Vector3.right, Vector3.forward);

        // ひとまず初期化
        dividedMesh = new Mesh();

        // 頂点群
        List<Vector3> verts = new List<Vector3>();
        source.GetVertices(verts);

        // 三角形群
        List<int> triangles = new List<int>();
        source.GetTriangles(triangles, submeshIndex);

        // 
        var vertArray = verts.ToArray();
        
        // 全頂点に対して平面の左右にあるかの処理をする
        foreach (var vert in vertArray)
        {
            
        }
    }

    // 辺を取得する
    /// <summary>
    /// 辺を取得する。{i, i + 1, i + 2, i + 3, ...} のように頂点のインデックスを格納している。
    /// i, i + 1 でペアでその次の i + 2, i + 3がペアである。
    /// </summary>
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
    /// <summary>
    /// Y軸方向：メッシュの最低点と最高点の高さの線分の中点の高さを返す
    /// </summary>
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
    /// <summary>
    /// X軸方向：メッシュの最低点【一番左】と最高点【一番右】の距離の線分の中点の高さを返す
    /// </summary>
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

    void Update()
    {
    }
}