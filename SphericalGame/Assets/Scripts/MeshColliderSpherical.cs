using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderSpherical : ColliderSpherical
{
    Mesh mesh;

    void Start()
    {
        Debug.Log(mesh.vertices.Length);
    }

    public override bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance)
    {
        return false;
    }
}
