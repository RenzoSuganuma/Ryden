using UnityEngine;
using BLINDED_AM_ME;

public class MeshCutterComponent : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private Material _material;
    private Ryden _jack = new Ryden();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var obj = _jack.CutMesh(_target, Vector3.zero, Vector3.right, _material);
        _jack.CutMesh(obj[0], Vector3.zero, Vector3.up, _material);
        _jack.CutMesh(obj[1], Vector3.zero, Vector3.up, _material);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
