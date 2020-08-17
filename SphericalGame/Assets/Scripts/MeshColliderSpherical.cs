using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderSpherical : ColliderSpherical
{
    public Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
    }

    public override bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance)
    {
        hitInfo = new RaycastHitSpherical();
        hitInfo.collider = this;
        hitInfo.distance = Mathf.Infinity;

        int[] idx = mesh.triangles;
        List<Vector4> pos = new List<Vector4>();
        mesh.GetUVs(1, pos);

        for (int i = 0; i < idx.Length; i += 3)
        {
            float[] coeffs = Rot4.CoCross(pos[i], pos[i + 1], pos[i + 2], ray.org, ray.dir);

            if (coeffs[0] < 0 && coeffs[1] < 0 && coeffs[2] < 0) // if the ray hits the front of the triangle
            {
                float distance = Mathf.Atan2(coeffs[4], coeffs[3]);
                distance = distance < 0 ? distance + 2 * Mathf.PI : distance; // wrap into range 0 .. 2pi
                
                if (distance < hitInfo.distance) // if this is the new best
                {
                    hitInfo.distance = distance;
                    hitInfo.triangleIndex = i / 3;
                }
            }
        }

        return hitInfo.distance <= maxDistance;
    }
}
