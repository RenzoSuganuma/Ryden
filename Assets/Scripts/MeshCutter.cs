using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    private MeshCutSide _leftside = new MeshCutSide();
    private MeshCutSide _rightside = new MeshCutSide();

    private Plane _blade;
    private Mesh _victimMesh;

    private List<Vector3> _newVerts = new List<Vector3>();

    public GameObject[] Cut(GameObject victim, Vector3 anchorPos, Vector3 normalDirection, Material capMat)
    {
        _blade = new Plane(victim.transform.InverseTransformDirection(-normalDirection),
            victim.transform.InverseTransformPoint(anchorPos)
        );

        _victimMesh = victim.GetComponent<MeshFilter>().mesh;

        _newVerts.Clear();

        _leftside.ClearAll();
        _rightside.ClearAll();

        bool[] sides = new bool[3];
        int[] indices;
        int p, p1, p2;

        for (int sub = 0; sub < _victimMesh.subMeshCount; ++sub)
        {
            indices = _victimMesh.GetIndices(sub);

            _leftside.SubIndices.Add(new List<int>());
            _rightside.SubIndices.Add(new List<int>());

            for (int i = 0; i < indices.Length; i += 3)
            {
                p = indices[i];
                p1 = indices[i + 1];
                p2 = indices[i + 2];

                sides[0] = _blade.GetSide(_victimMesh.vertices[p]);
                sides[1] = _blade.GetSide(_victimMesh.vertices[p1]);
                sides[2] = _blade.GetSide(_victimMesh.vertices[p2]);

                if (sides[0] == sides[1] && sides[0] == sides[2])
                {
                    if (sides[0])
                    {
                        _leftside.AddTriangle(p, p1, p2, sub, ref _victimMesh);
                    }
                    else
                    {
                        _leftside.AddTriangle(p, p1, p2, sub, ref _victimMesh);
                    }
                }
                else
                {
                    CutThisFace(sub, sides, p, p1, p2);
                }
            }
        }

        Material[] materials = victim.GetComponent<MeshRenderer>().sharedMaterials;

        if (materials[materials.Length - 1].name != capMat.name)
        {
            _leftside.SubIndices.Add(new List<int>());
            _rightside.SubIndices.Add(new List<int>());

            Material[] newMaterials = new Material[materials.Length - 1];

            materials.CopyTo(newMaterials, 0);

            newMaterials[materials.Length] = capMat;

            materials = newMaterials;
        }

        Capping();

        Mesh leftHalfMesh = new Mesh();
        leftHalfMesh.name = "split mesh left";
        leftHalfMesh.vertices = _leftside.Vectices.ToArray();
        leftHalfMesh.triangles = _leftside.Triangles.ToArray();
        leftHalfMesh.normals = _leftside.Normals.ToArray();
        leftHalfMesh.uv = _leftside.Uvs.ToArray();

        leftHalfMesh.subMeshCount = _leftside.SubIndices.Count;

        for (int i = 0; i < _leftside.SubIndices.Count; i++)
        {
            leftHalfMesh.SetIndices(_leftside.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        Mesh rightHalfMesh = new Mesh();
        rightHalfMesh.name = "split mesh left";
        rightHalfMesh.vertices = _rightside.Vectices.ToArray();
        rightHalfMesh.triangles = _rightside.Triangles.ToArray();
        rightHalfMesh.normals = _rightside.Normals.ToArray();
        rightHalfMesh.uv = _rightside.Uvs.ToArray();

        rightHalfMesh.subMeshCount = _rightside.SubIndices.Count;

        for (int i = 0; i < _rightside.SubIndices.Count; i++)
        {
            leftHalfMesh.SetIndices(_rightside.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        victim.name = "left side";
        victim.GetComponent<MeshFilter>().mesh = leftHalfMesh;

        GameObject leftSideObj = victim;

        GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer));
        rightSideObj.transform.position = victim.transform.position;
        rightSideObj.transform.rotation = victim.transform.rotation;
        rightSideObj.GetComponent<MeshFilter>().mesh = rightHalfMesh;

        leftSideObj.GetComponent<MeshRenderer>().materials = materials;
        rightSideObj.GetComponent<MeshRenderer>().materials = materials;

        return new GameObject[] { leftSideObj, rightSideObj };
    }

    private void CutThisFace(int submesh, bool[] sides, int p, int p1, int p2)
    {
        Vector3[] leftPoints = new Vector3[2];
        Vector3[] leftNormals = new Vector3[2];
        Vector2[] leftUvs = new Vector2[2];

        Vector3[] rightPoints = new Vector3[2];
        Vector3[] rightNormals = new Vector3[2];
        Vector2[] rightUvs = new Vector2[2];

        bool leftSetted = false;
        bool rightSetted = false;

        int pnt = p;
        for (int side = 0; side < 3; ++side)
        {
            switch (side)
            {
                case 0: pnt = p;
                    break;
                case 1: pnt = p1;
                    break;
                case 2: pnt = p2;
                    break;
            }

            if (sides[side])
            {
                if (!leftSetted)
                {
                    leftSetted = true;

                    leftPoints[0] = _victimMesh.vertices[pnt];
                    leftPoints[1] = leftPoints[0];

                    leftUvs[0] = _victimMesh.uv[pnt];
                    leftUvs[1] = leftUvs[0];

                    leftNormals[0] = _victimMesh.normals[pnt];
                    leftNormals[1] = leftNormals[0];
                }
                else
                {
                    leftPoints[1] = _victimMesh.vertices[pnt];
                    leftUvs[1] = _victimMesh.uv[pnt];
                    leftNormals[1] = _victimMesh.normals[pnt];
                }
            }
            else
            {
                if (!rightSetted)
                {
                    rightSetted = true;

                    rightPoints[0] = _victimMesh.vertices[pnt];
                    rightPoints[1] = rightPoints[0];
                    
                    rightUvs[0] = _victimMesh.uv[pnt];
                    rightUvs[1] = rightUvs[0];

                    rightNormals[0] = _victimMesh.normals[pnt];
                    rightNormals[1] = rightNormals[0];
                }
                else
                {
                    rightPoints[1] = _victimMesh.vertices[pnt];
                    rightUvs[1] = _victimMesh.uv[pnt];
                    rightNormals[1] = _victimMesh.normals[pnt];
                }
            }

            float normalizedDistance = 0f;
            float distance = 0f;
            
            
        }
    }

    private void Capping()
    {
    }
}

public class MeshCutSide
{
    public List<Vector3> Vectices = new List<Vector3>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector2> Uvs = new List<Vector2>();
    public List<int> Triangles = new List<int>();
    public List<List<int>> SubIndices = new List<List<int>>();

    public void ClearAll()
    {
        Vectices.Clear();
        Normals.Clear();
        Uvs.Clear();
        Triangles.Clear();
        SubIndices.Clear();
    }

    public void AddTriangle(int p, int p1, int p2, int submesh, ref Mesh victimMesh)
    {
        int baseindex = Vectices.Count;

        SubIndices[submesh].Add(baseindex);
        SubIndices[submesh].Add(baseindex + 1);
        SubIndices[submesh].Add(baseindex + 2);

        Triangles.Add(baseindex);
        Triangles.Add(baseindex + 1);
        Triangles.Add(baseindex + 2);

        Normals.Add(victimMesh.vertices[p]);
        Normals.Add(victimMesh.vertices[p1]);
        Normals.Add(victimMesh.vertices[p2]);

        Uvs.Add(victimMesh.uv[p]);
        Uvs.Add(victimMesh.uv[p1]);
        Uvs.Add(victimMesh.uv[p2]);
    }

    public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int submesh)
    {
        Vector3 calculatedNormal =
            Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

        int p, p1, p2;
        p = p1 = p2 = 0;

        if (Vector3.Dot(calculatedNormal, faceNormal) < 0)
        {
            p = 2;
            p1 = 1;
            p2 = 0;
        }

        int baseIndex = Vectices.Count;

        SubIndices[submesh].Add(baseIndex);
        SubIndices[submesh].Add(baseIndex + 1);
        SubIndices[submesh].Add(baseIndex + 2);

        Triangles.Add(baseIndex);
        Triangles.Add(baseIndex + 1);
        Triangles.Add(baseIndex + 2);

        Vectices.Add(points3[p]);
        Vectices.Add(points3[p1]);
        Vectices.Add(points3[p2]);

        Normals.Add(normals3[p]);
        Normals.Add(normals3[p1]);
        Normals.Add(normals3[p2]);

        Uvs.Add(uvs3[p]);
        Uvs.Add(uvs3[p1]);
        Uvs.Add(uvs3[p2]);
    }
}