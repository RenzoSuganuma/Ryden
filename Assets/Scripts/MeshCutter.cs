using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private Material _material;

    private MeshCuttedSide _leftSide = new MeshCuttedSide();
    private MeshCuttedSide _rightSide = new MeshCuttedSide();

    private Plane _bladePlane;
    private Mesh _cutTargetMesh;

    private List<Vector3> _newVerts = new List<Vector3>();

    private List<Vector3> _capVertChecked = new List<Vector3>();

    private List<Vector3> _capVertPolygon = new List<Vector3>();

    private void Start()
    {
        _leftSide.TargetMesh = _target.GetComponent<MeshFilter>().mesh;
        _rightSide.TargetMesh = _target.GetComponent<MeshFilter>().mesh;
        var obj = Cut(_target, Vector3.zero, Vector3.forward, _material);
    }

    public List<GameObject> Cut(GameObject target, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial)
    {
        _bladePlane = new Plane(Vector3.right, Vector3.forward);

        _cutTargetMesh = _target.GetComponent<MeshFilter>().mesh;

        _newVerts.Clear();

        _leftSide.ClearAll();

        _rightSide.ClearAll();

        bool[] sides = new bool[3];
        int[] indices;
        int p0, p1, p2;

        for (int sub = 0; sub < _cutTargetMesh.subMeshCount; ++sub)
        {
            indices = _cutTargetMesh.GetIndices(sub);

            _leftSide.SubIndices.Add(new List<int>());
            _rightSide.SubIndices.Add(new List<int>());

            for (int i = 0; i < indices.Length; ++i)
            {
                p0 = indices[i];
                p1 = indices[i + 1];
                p2 = indices[i + 2];

                sides[0] = _bladePlane.GetSide(_cutTargetMesh.vertices[p0]);
                sides[1] = _bladePlane.GetSide(_cutTargetMesh.vertices[p1]);
                sides[2] = _bladePlane.GetSide(_cutTargetMesh.vertices[p2]);

                if (sides[0] == sides[1] && sides[0] == sides[2])
                {
                    if (sides[0])
                    {
                        _leftSide.AddTriangle(p0, p1, p2, sub);
                    }
                    else
                    {
                        _rightSide.AddTriangle(p0, p1, p2, sub);
                    }
                }
                else
                {
                    CutThisFace(sub, sides, p0, p1, p2);
                }
            }
        }

        Material[] materials = _target.GetComponent<MeshRenderer>().sharedMaterials;

        if (materials[materials.Length - 1].name != capMaterial.name)
        {
            _leftSide.SubIndices.Add(new List<int>());
            _rightSide.SubIndices.Add(new List<int>());

            Material[] newMats = new Material[materials.Length + 1];

            materials.CopyTo(newMats, 0);

            newMats[materials.Length] = capMaterial;

            materials = newMats;
        }

        Capping();

        Mesh leftHMesh = new Mesh();
        leftHMesh.name = "Splitted-Mesh-Left";
        leftHMesh.vertices = _leftSide.Vertices.ToArray();
        leftHMesh.triangles = _leftSide.Triangles.ToArray();
        leftHMesh.normals = _leftSide.Normals.ToArray();
        leftHMesh.uv = _leftSide.Uvs.ToArray();

        leftHMesh.subMeshCount = _leftSide.SubIndices.Count;
        for (int i = 0; i < _leftSide.SubIndices.Count; ++i)
        {
            leftHMesh.SetIndices(_leftSide.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        Mesh rightHMesh = new Mesh();
        rightHMesh.name = "Splitted-Mesh-Left";
        rightHMesh.vertices = _leftSide.Vertices.ToArray();
        rightHMesh.triangles = _leftSide.Triangles.ToArray();
        rightHMesh.normals = _leftSide.Normals.ToArray();
        rightHMesh.uv = _leftSide.Uvs.ToArray();

        rightHMesh.subMeshCount = _leftSide.SubIndices.Count;
        for (int i = 0; i < _leftSide.SubIndices.Count; ++i)
        {
            rightHMesh.SetIndices(_leftSide.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        _target.name = "Sliced-Left-Side";
        _target.GetComponent<MeshFilter>().mesh = leftHMesh;

        GameObject leftSideObj = _target;

        GameObject rightSideObj = new GameObject("Right-Side", typeof(MeshFilter), typeof(MeshRenderer));
        rightSideObj.transform.position = _target.transform.position;
        rightSideObj.transform.rotation = _target.transform.rotation;
        rightSideObj.GetComponent<MeshFilter>().mesh = rightHMesh;

        leftSideObj.GetComponent<MeshRenderer>().materials = materials;
        rightSideObj.GetComponent<MeshRenderer>().materials = materials;

        return new List<GameObject>() { leftSideObj, rightSideObj };
    }

    private void CutThisFace(int submesh, bool[] sides, int index0, int index1, int index2)
    {
        Vector3[] leftPoints = new Vector3[2];
        Vector3[] leftNormals = new Vector3[2];
        Vector2[] leftUvs = new Vector2[2];

        Vector3[] rightPoints = new Vector3[2];
        Vector3[] rightNormals = new Vector3[2];
        Vector2[] rightUvs = new Vector2[2];

        bool leftSetted = false;
        bool rightSetted = false;

        int p = index0;
        for (int side = 0; side < 3; ++side)
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
                if (!leftSetted)
                {
                    leftSetted = true;

                    leftPoints[0] = _cutTargetMesh.vertices[p];
                    leftPoints[1] = leftPoints[0];

                    leftUvs[0] = _cutTargetMesh.uv[p];
                    leftUvs[1] = leftUvs[0];

                    leftNormals[0] = _cutTargetMesh.normals[p];
                    leftNormals[1] = leftNormals[0];
                }
                else
                {
                    leftPoints[1] = _cutTargetMesh.vertices[p];
                    leftUvs[1] = _cutTargetMesh.uv[p];
                    leftNormals[1] = _cutTargetMesh.normals[p];
                }
            }
            else
            {
                if (!rightSetted)
                {
                    rightSetted = true;

                    rightPoints[0] = _cutTargetMesh.vertices[p];
                    rightPoints[1] = rightPoints[0];

                    rightUvs[0] = _cutTargetMesh.uv[p];
                    rightUvs[1] = rightUvs[0];

                    rightNormals[0] = _cutTargetMesh.normals[p];
                    rightNormals[1] = rightNormals[0];
                }
                else
                {
                    rightPoints[1] = _cutTargetMesh.vertices[p];
                    rightUvs[1] = _cutTargetMesh.uv[p];
                    rightNormals[1] = _cutTargetMesh.normals[p];
                }
            }
        }

        float normalizedDistance = 0f;
        float distance = 0f;

        _bladePlane.Raycast(new Ray(leftPoints[0], (rightPoints[0] - leftPoints[0]).normalized), out distance);

        normalizedDistance = distance / (rightPoints[0] - leftPoints[0]).magnitude;

        Vector3 newVertex0 = Vector3.Lerp(leftPoints[0], rightPoints[0], normalizedDistance);
        Vector2 newUv0 = Vector2.Lerp(leftUvs[0], rightUvs[0], normalizedDistance);
        Vector3 newNormal0 = Vector3.Lerp(leftNormals[0], rightNormals[0], normalizedDistance);

        _newVerts.Add(newVertex0);

        _bladePlane.Raycast(new Ray(leftPoints[1], (rightPoints[1] - leftPoints[1]).normalized), out distance);

        normalizedDistance = distance / (rightPoints[1] - leftPoints[1]).magnitude;

        Vector3 newVertex1 = Vector3.Lerp(leftPoints[1], rightPoints[1], normalizedDistance);
        Vector2 newUv1 = Vector2.Lerp(leftUvs[1], rightUvs[1], normalizedDistance);
        Vector3 newNormal1 = Vector3.Lerp(leftNormals[1], rightNormals[1], normalizedDistance);

        _newVerts.Add(newVertex1);

        _leftSide.AddTriangle(
            new Vector3[] { leftPoints[0], newVertex0, newVertex1 },
            new Vector3[] { leftNormals[0], newNormal0, newNormal1 },
            new Vector2[] { leftUvs[0], newUv0, newUv1 },
            newNormal0,
            submesh
        );

        _leftSide.AddTriangle(
            new Vector3[] { leftPoints[0], leftPoints[1], newVertex1 },
            new Vector3[] { leftNormals[0], leftPoints[1], newNormal1 },
            new Vector2[] { leftUvs[0], leftUvs[1], newUv1 },
            newNormal1,
            submesh
        );

        _rightSide.AddTriangle(
            new Vector3[] { rightPoints[0], newVertex0, newVertex1 },
            new Vector3[] { rightNormals[0], newNormal0, newNormal1 },
            new Vector2[] { rightUvs[0], newUv0, newUv1 },
            newNormal0,
            submesh
        );

        _rightSide.AddTriangle(
            new Vector3[] { rightPoints[0], rightPoints[1], newVertex1 },
            new Vector3[] { rightNormals[0], rightNormals[1], newNormal1 },
            new Vector2[] { rightUvs[0], rightUvs[1], newUv1 },
            newNormal0,
            submesh
        );

        _capVertChecked = new List<Vector3>();
        _capVertPolygon = new List<Vector3>();
    }

    private void Capping()
    {
        _capVertChecked.Clear();

        for (int i = 0; i < _newVerts.Count; ++i)
        {
            if (_capVertChecked.Contains(_newVerts[i]))
            {
                continue;
            }

            _capVertPolygon.Clear();

            _capVertPolygon.Add(_newVerts[i]);
            _capVertPolygon.Add(_newVerts[i + 1]);

            _capVertChecked.Add(_newVerts[i]);
            _capVertChecked.Add(_newVerts[i + 1]);

            bool isdone = false;
            while (!isdone)
            {
                isdone = true;

                for (int k = 0; k < _newVerts.Count; k += 2)
                {
                    if (_newVerts[k] == _capVertPolygon[_capVertPolygon.Count - 1] &&
                        !_capVertChecked.Contains(_newVerts[k + 1]))
                    {
                        isdone = false;
                        _capVertPolygon.Add(_newVerts[k + 1]);
                        _capVertChecked.Add(_newVerts[k + 1]);
                    }
                    else if (_newVerts[k + 1] == _capVertPolygon[_capVertPolygon.Count - 1] &&
                             !_capVertChecked.Contains(_newVerts[k]))
                    {
                        isdone = false;
                        _capVertPolygon.Add(_newVerts[k]);
                        _capVertChecked.Add(_newVerts[k]);
                    }
                }
            }

            FillCap(_capVertPolygon);
        }
    }

    private void FillCap(List<Vector3> vertices)
    {
        Vector3 center = Vector3.zero;

        foreach (var vertex in vertices)
        {
            center += vertex;
        }

        center /= vertices.Count;

        Vector3 upward = Vector3.zero;
        upward.x = _bladePlane.normal.y;
        upward.y = -_bladePlane.normal.x;
        upward.z = _bladePlane.normal.z;

        Vector3 left = Vector3.Cross(_bladePlane.normal, upward);

        Vector3 displacement = Vector3.zero;
        Vector3 newUv0 = Vector3.zero;
        Vector3 newUv1 = Vector3.zero;

        for (int i = 0; i < vertices.Count; ++i)
        {
            displacement = vertices[i] - center;

            newUv0 = Vector3.zero;
            newUv0.x = .5f + Vector3.Dot(displacement, left);
            newUv0.y = .5f + Vector3.Dot(displacement, upward);
            newUv0.z = .5f + Vector3.Dot(displacement, _bladePlane.normal);

            displacement = vertices[(i + 1) % vertices.Count] - center;

            newUv1.x = .5f + Vector3.Dot(displacement, left);
            newUv1.y = .5f + Vector3.Dot(displacement, upward);
            newUv1.z = .5f + Vector3.Dot(displacement, _bladePlane.normal);

            _leftSide.AddTriangle(
                new Vector3[]
                {
                    vertices[i],
                    vertices[(i + 1) % vertices.Count],
                    center
                },
                new Vector3[]
                {
                    -_bladePlane.normal,
                    -_bladePlane.normal,
                    -_bladePlane.normal
                },
                new Vector2[]
                {
                    newUv0,
                    newUv1,
                    Vector2.one * .5f
                },
                -_bladePlane.normal,
                _leftSide.SubIndices.Count - 1
            );

            _rightSide.AddTriangle(
                new Vector3[]
                {
                    vertices[i],
                    vertices[(i + 1) % vertices.Count],
                    center
                },
                new Vector3[]
                {
                    _bladePlane.normal,
                    _bladePlane.normal,
                    _bladePlane.normal
                },
                new Vector2[]
                {
                    newUv0,
                    newUv1,
                    Vector2.one * .5f
                },
                _bladePlane.normal,
                _rightSide.SubIndices.Count - 1
            );
        }
    }
}

public class MeshCuttedSide
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector2> Uvs = new List<Vector2>();
    public List<int> Triangles = new List<int>();
    public List<List<int>> SubIndices = new List<List<int>>();

    public Mesh TargetMesh
    {
        get { return _mesh; }
        set { _mesh = value; }
    }

    private Mesh _mesh;

    public void ClearAll()
    {
        Vertices.Clear();
        Normals.Clear();
        Uvs.Clear();
        Triangles.Clear();
        SubIndices.Clear();
    }

    public void AddTriangle(int p0, int p1, int p2, int submesh)
    {
        int baseIndex = Vertices.Count;

        SubIndices[submesh].Add(baseIndex);
        SubIndices[submesh].Add(baseIndex + 1);
        SubIndices[submesh].Add(baseIndex + 2);

        Triangles.Add(baseIndex);
        Triangles.Add(baseIndex + 1);
        Triangles.Add(baseIndex + 2);

        Vertices.Add(_mesh.vertices[p0]);
        Vertices.Add(_mesh.vertices[p1]);
        Vertices.Add(_mesh.vertices[p2]);

        Normals.Add(_mesh.normals[p0]);
        Normals.Add(_mesh.normals[p1]);
        Normals.Add(_mesh.normals[p2]);

        Uvs.Add(_mesh.uv[p0]);
        Uvs.Add(_mesh.uv[p1]);
        Uvs.Add(_mesh.uv[p2]);
    }

    public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int submesh)
    {
        Vector3 calculatedNormal =
            Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

        int p0 = 0;
        int p1 = 1;
        int p2 = 2;

        if (Vector3.Dot(calculatedNormal, faceNormal) < 0f)
        {
            p0 = 2;
            p1 = 1;
            p2 = 0;
        }

        int baseIndex = Vertices.Count;

        SubIndices[submesh].Add(baseIndex);
        SubIndices[submesh].Add(baseIndex + 1);
        SubIndices[submesh].Add(baseIndex + 2);

        Triangles.Add(baseIndex);
        Triangles.Add(baseIndex + 1);
        Triangles.Add(baseIndex + 2);

        Vertices.Add(points3[p0]);
        Vertices.Add(points3[p1]);
        Vertices.Add(points3[p2]);

        Normals.Add(normals3[p0]);
        Normals.Add(normals3[p1]);
        Normals.Add(normals3[p2]);

        Uvs.Add(uvs3[p0]);
        Uvs.Add(uvs3[p1]);
        Uvs.Add(uvs3[p2]);
    }
}