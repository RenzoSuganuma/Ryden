using UnityEngine;
using BLINDED_AM_ME;

public class MeshCutterComponent : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private Material _material;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var obj = Ryden.CutMesh(_target, Vector3.zero, Vector3.right, _material);
        Ryden.CutMesh(obj[0], Vector3.zero, Vector3.up, _material);
        Ryden.CutMesh(obj[1], Vector3.zero, Vector3.up, _material);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
