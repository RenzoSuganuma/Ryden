using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 切断されたメッシュ
/// </summary>
public class CuttedMesh
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector2> UVs = new List<Vector2>();
    public List<int> Triangles = new List<int>();
    public List<List<int>> SubIndices = new List<List<int>>();

    /// <summary>
    /// このクラスに保持しているデータをすべて消す
    /// </summary>
    public void ClearAll()
    {
        Vertices.Clear();
        Normals.Clear();
        UVs.Clear();
        Triangles.Clear();
        SubIndices.Clear();
    }

    /// <summary>
    /// トライアングルとして3頂点を追加する
    /// </summary>
    public void AddTriangle(int p0, int p1, int p2, int subMesh, ref Mesh victimMesh)
    {
        int baseIndex = Vertices.Count;

        // 対象のサブメッシュのインデックスへ追加
        SubIndices[subMesh].Add(baseIndex);
        SubIndices[subMesh].Add(baseIndex + 1);
        SubIndices[subMesh].Add(baseIndex + 2);

        // 三角形群の設定
        Triangles.Add(baseIndex);
        Triangles.Add(baseIndex + 1);
        Triangles.Add(baseIndex + 2);

        // 対象メッシュから頂点データを取得する   
        Vertices.Add(victimMesh.vertices[p0]);
        Vertices.Add(victimMesh.vertices[p1]);
        Vertices.Add(victimMesh.vertices[p2]);

        // 法線も同様に取得
        Normals.Add(victimMesh.normals[p0]);
        Normals.Add(victimMesh.normals[p1]);
        Normals.Add(victimMesh.normals[p2]);

        // UVもどう容易に取得
        UVs.Add(victimMesh.uv[p0]);
        UVs.Add(victimMesh.uv[p1]);
        UVs.Add(victimMesh.uv[p2]);
    }

    /// <summary>
    /// トライアングルの追加。ここではポリゴンを渡し、それを追加する
    /// </summary>
    public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int subMesh)
    {
        Vector3 normalCalculated =
            Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

        int p0, p1, p2;

        p0 = 0;
        p1 = 1;
        p2 = 2;

        // 法線とトライアングルが逆の場合には面を裏返す
        if (Vector3.Dot(normalCalculated, faceNormal) < 0)
        {
            p0 = 2;
            // p1 は真ん中のためここで初期化しなくてもよい
            p2 = 0;
        }

        int baseIndex = Vertices.Count;

        SubIndices[subMesh].Add(baseIndex);
        SubIndices[subMesh].Add(baseIndex + 1);
        SubIndices[subMesh].Add(baseIndex + 2);

        Triangles.Add(baseIndex);
        Triangles.Add(baseIndex + 1);
        Triangles.Add(baseIndex + 2);

        Vertices.Add(points3[p0]);
        Vertices.Add(points3[p1]);
        Vertices.Add(points3[p2]);

        Normals.Add(normals3[p0]);
        Normals.Add(normals3[p1]);
        Normals.Add(normals3[p2]);

        UVs.Add(uvs3[p0]);
        UVs.Add(uvs3[p1]);
        UVs.Add(uvs3[p2]);
    }
}

/// <summary>
/// メッシュ切断機能を提供する
/// </summary>
public class Ryden
{
    private CuttedMesh _leftCuttedMesh = new CuttedMesh();
    private CuttedMesh _rightCuttedMesh = new CuttedMesh();
    private Plane _blade;
    private Mesh _victimMesh;
    private List<Vector3> _newVertices = new List<Vector3>();

    /// <summary>
    /// メッシュを切断し、切断されたメッシュを返す
    /// </summary>
    /// <param name="victim">切断対象のゲームオブジェクト</param>
    /// <param name="anchorPos">切断面のアンカー位置</param>
    /// <param name="normalDir">切断面の法線</param>
    /// <param name="capMat">切断面のマテリアル</param>
    /// <returns></returns>
    public GameObject[] CutMesh(GameObject victim, Vector3 anchorPos, Vector3 normalDir, Material capMat)
    {
        // 対象のローカル座標から平面を生成
        _blade = new Plane(
            victim.transform.InverseTransformDirection(-normalDir),
            victim.transform.InverseTransformPoint(anchorPos)
        );

        _victimMesh = victim.GetComponent<MeshFilter>().mesh;
        _newVertices.Clear();
        _leftCuttedMesh.ClearAll();
        _rightCuttedMesh.ClearAll();

        bool[] sides = new bool[3]; // 平面の左右にあるかのフラグ
        int[] indices;
        int p0, p1, p2;

        // サブメッシュの数だけループして切断処理をする
        for (int submesh = 0; submesh < _victimMesh.subMeshCount; submesh++)
        {
            indices = _victimMesh.GetIndices(submesh);

            _leftCuttedMesh.SubIndices.Add(new List<int>());
            _rightCuttedMesh.SubIndices.Add(new List<int>());

            // サブメッシュのインデックス数分ループ
            for (int i = 0; i < indices.Length; i += 3)
            {
                p0 = indices[i];
                p1 = indices[i + 1];
                p2 = indices[i + 2];

                sides[0] = _blade.GetSide(_victimMesh.vertices[p0]);
                sides[1] = _blade.GetSide(_victimMesh.vertices[p1]);
                sides[2] = _blade.GetSide(_victimMesh.vertices[p2]);

                // すべて切断面の左右にある場合には切断処理をしない
                if (sides[0] == sides[1] && sides[0] == sides[2])
                {
                    // 左右にあるかに応じ、トライアングルの追加
                    if (sides[0])
                    {
                        _leftCuttedMesh.AddTriangle(p0, p1, p2, submesh, ref _victimMesh);
                    }
                    else
                    {
                        _rightCuttedMesh.AddTriangle(p0, p1, p2, submesh, ref _victimMesh);
                    }
                }
                else
                {
                    // 切断をする
                    CutThisFace(submesh, sides, p0, p1, p2);
                }
            }
        }

        Material[] materials = victim.GetComponent<MeshRenderer>().sharedMaterials;

        if (materials[materials.Length - 1].name != capMat.name)
        {
            _leftCuttedMesh.SubIndices.Add(new List<int>());
            _rightCuttedMesh.SubIndices.Add(new List<int>());

            Material[] newMaterials = new Material[materials.Length + 1];

            materials.CopyTo(newMaterials, 0);

            newMaterials[materials.Length] = capMat;

            materials = newMaterials;
        }

        // カット処理
        Capping();

        // 左側のメッシュを生成
        Mesh leftHalfMesh = new Mesh();
        leftHalfMesh.name = "Left Splitted";
        leftHalfMesh.vertices = _leftCuttedMesh.Vertices.ToArray();
        leftHalfMesh.triangles = _leftCuttedMesh.Triangles.ToArray();
        leftHalfMesh.normals = _leftCuttedMesh.Normals.ToArray();
        leftHalfMesh.uv = _leftCuttedMesh.UVs.ToArray();

        leftHalfMesh.subMeshCount = _leftCuttedMesh.SubIndices.Count;
        for (int i = 0; i < _leftCuttedMesh.SubIndices.Count; i++)
        {
            leftHalfMesh.SetIndices(_leftCuttedMesh.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        // 右側のメッシュを生成
        Mesh rightHalfMesh = new Mesh();
        rightHalfMesh.name = "Right Splitted";
        rightHalfMesh.vertices = _rightCuttedMesh.Vertices.ToArray();
        rightHalfMesh.triangles = _rightCuttedMesh.Triangles.ToArray();
        rightHalfMesh.normals = _rightCuttedMesh.Normals.ToArray();
        rightHalfMesh.uv = _rightCuttedMesh.UVs.ToArray();

        rightHalfMesh.subMeshCount = _rightCuttedMesh.SubIndices.Count;
        for (int i = 0; i < _rightCuttedMesh.SubIndices.Count; i++)
        {
            rightHalfMesh.SetIndices(_rightCuttedMesh.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        // 元のオブジェクトを左側に
        victim.name = "Left Side";
        victim.GetComponent<MeshFilter>().mesh = leftHalfMesh;

        // 右側は生成
        GameObject leftObj = victim;

        GameObject rightObj = new GameObject("Right Side", typeof(MeshFilter), typeof(MeshRenderer));
        rightObj.transform.position = victim.transform.position;
        rightObj.transform.rotation = victim.transform.rotation;
        rightObj.GetComponent<MeshFilter>().mesh = rightHalfMesh;

        leftObj.GetComponent<MeshRenderer>().materials = materials;
        rightObj.GetComponent<MeshRenderer>().materials = materials;

        return new GameObject[] { leftObj, rightObj };
    }

    private void CutThisFace(int subMesh, bool[] sides, int index0, int index1, int index2)
    {
        Vector3[] leftPoints = new Vector3[2];
        Vector3[] leftNormals = new Vector3[2];
        Vector2[] leftUVs = new Vector2[2];

        Vector3[] rightPoints = new Vector3[2];
        Vector3[] rightNormals = new Vector3[2];
        Vector2[] rightUVs = new Vector2[2];

        bool settedLeft = false;
        bool settedRight = false;

        int p = index0;

        for (int side = 0; side < 3; side++)
        {
            switch (side)
            {
                case 0:
                    p = index0;
                    break;
                case 1:
                    p = index1;
                    break;
                case 2:
                    p = index2;
                    break;
            }

            if (sides[side])
            {
                if (!settedLeft)
                {
                    settedLeft = true;

                    leftPoints[0] = _victimMesh.vertices[p];
                    leftPoints[1] = leftPoints[0];

                    leftUVs[0] = _victimMesh.uv[p];
                    leftUVs[1] = leftUVs[0];

                    leftNormals[0] = _victimMesh.normals[p];
                    leftNormals[1] = leftNormals[0];
                }
                else
                {
                    leftPoints[1] = _victimMesh.vertices[p];
                    leftUVs[1] = _victimMesh.uv[p];
                    leftNormals[1] = _victimMesh.normals[p];
                }
            }
            else
            {
                if (!settedRight)
                {
                    settedRight = true;

                    rightPoints[0] = _victimMesh.vertices[p];
                    rightPoints[1] = rightPoints[0];

                    rightUVs[0] = _victimMesh.uv[p];
                    rightUVs[1] = rightUVs[0];

                    rightNormals[0] = _victimMesh.normals[p];
                    rightNormals[1] = rightNormals[0];
                }
                else
                {
                    rightPoints[1] = _victimMesh.vertices[p];
                    rightUVs[1] = _victimMesh.uv[p];
                    rightNormals[1] = _victimMesh.normals[p];
                }
            }
        }

        float normalizedDistance = 0f;
        float distance = 0f;

        // 左側
        _blade.Raycast(new Ray(leftPoints[0], (rightPoints[0] - leftPoints[0]).normalized), out distance);

        normalizedDistance = distance / (rightPoints[0] - leftPoints[0]).magnitude;

        Vector3 newVertex1 = Vector3.Lerp(leftPoints[0], rightPoints[0], normalizedDistance);
        Vector2 newUv1 = Vector2.Lerp(leftUVs[0], rightUVs[0], normalizedDistance);
        Vector3 newNormal1 = Vector3.Lerp(leftNormals[0], rightNormals[0], normalizedDistance);

        _newVertices.Add(newVertex1);

        // 右側
        _blade.Raycast(new Ray(leftPoints[1], (rightPoints[1] - leftPoints[1]).normalized), out distance);

        normalizedDistance = distance / (rightPoints[1] - leftPoints[1]).magnitude;

        Vector3 newVertex2 = Vector3.Lerp(leftPoints[1], rightPoints[1], normalizedDistance);
        Vector2 newUv2 = Vector2.Lerp(leftUVs[1], rightUVs[1], normalizedDistance);
        Vector3 newNormal2 = Vector3.Lerp(leftNormals[1], rightNormals[1], normalizedDistance);

        _newVertices.Add(newVertex2);

        // トライアングル
        // 左側
        _leftCuttedMesh.AddTriangle(
            new Vector3[] { leftPoints[0], newVertex1, newVertex2 },
            new Vector3[] { leftNormals[0], newNormal1, newNormal2 },
            new Vector2[] { leftUVs[0], newUv1, newUv2 },
            newNormal1,
            subMesh
        );

        _leftCuttedMesh.AddTriangle(
            new Vector3[] { leftPoints[0], leftPoints[1], newVertex2 },
            new Vector3[] { leftNormals[0], leftNormals[1], newNormal2 },
            new Vector2[] { leftUVs[0], leftUVs[1], newUv2 },
            newNormal2,
            subMesh
        );
        
        // 右側
        _rightCuttedMesh.AddTriangle(
            new Vector3[] { rightPoints[0], newVertex1, newVertex2 },
            new Vector3[] { rightNormals[0], newNormal1, newNormal2 },
            new Vector2[] { rightUVs[0], newUv1, newUv2 },
            newNormal1,
            subMesh
        );

        _rightCuttedMesh.AddTriangle(
            new Vector3[] { rightPoints[0], rightPoints[1], newVertex2 },
            new Vector3[] { rightNormals[0], rightNormals[1], newNormal2 },
            new Vector2[] { rightUVs[0], rightUVs[1], newUv2 },
            newNormal2,
            subMesh
        );
    }

    private List<Vector3> _capVertChecked = new List<Vector3>();
    private List<Vector3> _capVertPolygon = new List<Vector3>();

    private void Capping()
    {
        _capVertChecked.Clear();

        for (int i = 0; i < _newVertices.Count; i++)
        {
            if (_capVertChecked.Contains(_newVertices[i]))
            {
                continue;
            }

            _capVertPolygon.Clear();

            _capVertPolygon.Add(_newVertices[i]);
            _capVertPolygon.Add(_newVertices[i + 1]);

            _capVertChecked.Add(_newVertices[i]);
            _capVertChecked.Add(_newVertices[i + 1]);

            bool isDone = false;
            while (!isDone)
            {
                isDone = true;
                
                for (int k = 0; k < _newVertices.Count; k += 2)
                {
                    if (_newVertices[k] == _capVertPolygon[_capVertPolygon.Count - 1] &&
                        !_capVertChecked.Contains(_newVertices[k + 1]))
                    {
                        isDone = false;
                        _capVertPolygon.Add(_newVertices[k + 1]);
                        _capVertChecked.Add(_newVertices[k + 1]);
                    }
                    else if (_newVertices[k + 1] == _capVertPolygon[_capVertPolygon.Count - 1] &&
                             !_capVertChecked.Contains(_newVertices[k]))
                    {
                        isDone = false;
                        _capVertPolygon.Add(_newVertices[k]);
                        _capVertChecked.Add(_newVertices[k]);
                    }
                }
            }

            FillCap(_capVertPolygon);
        }
    }

    private void FillCap(List<Vector3> verts)
    {
        Vector3 center = Vector3.zero;

        foreach (var vert in verts)
        {
            center += vert;
        }

        center /= verts.Count;

        Vector3 upward = Vector3.zero;

        upward.x = _blade.normal.y;
        upward.y = -_blade.normal.x;
        upward.z = _blade.normal.z;

        Vector3 left = Vector3.Cross(_blade.normal, upward);

        Vector3 displacement = Vector3.zero;
        Vector3 newUv1 = Vector3.zero;
        Vector3 newUv2 = Vector3.zero;

        for (int i = 0; i < verts.Count; i++)
        {
            displacement = verts[i] - center;

            newUv1 = Vector3.zero;
            newUv1.x = .5f + Vector3.Dot(displacement, left);
            newUv1.y = .5f + Vector3.Dot(displacement, upward);
            newUv1.z = .5f + Vector3.Dot(displacement, _blade.normal);

            displacement = verts[(i + 1) % verts.Count] - center;

            newUv2 = Vector3.zero;
            newUv2.x = .5f + Vector3.Dot(displacement, left);
            newUv2.y = .5f + Vector3.Dot(displacement, upward);
            newUv2.z = .5f + Vector3.Dot(displacement, _blade.normal);

            _leftCuttedMesh.AddTriangle(
                new Vector3[]
                {
                    verts[i],
                    verts[(i + 1) % verts.Count],
                    center
                },
                new Vector3[]
                {
                    -_blade.normal,
                    -_blade.normal,
                    -_blade.normal
                },
                new Vector2[]
                {
                    newUv1,
                    newUv2,
                    Vector2.one * .5f
                },
                -_blade.normal,
                _leftCuttedMesh.SubIndices.Count - 1
            );

            _rightCuttedMesh.AddTriangle(
                new Vector3[]
                {
                    verts[i],
                    verts[(i + 1) % verts.Count],
                    center
                },
                new Vector3[]
                {
                    _blade.normal,
                    _blade.normal,
                    _blade.normal
                },
                new Vector2[]
                {
                    newUv1,
                    newUv2,
                    Vector2.one * .5f
                },
                _blade.normal,
                _rightCuttedMesh.SubIndices.Count - 1
            );
        }
    }
}