using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlmSharp;

public class MeshColliderSpherical : ColliderSpherical
{
    private Mesh _mesh;
    private int[] idx;
    private List<Vector4> pos;
    private mat4x3[] baryFace;
    // if triangle n is i, j, k, then baryEdge[3n : 3n + 3] cooresponds to edges ij, jk, ki resp.
    private mat4x2[] baryEdge;

    public Mesh mesh
    {
        get => _mesh;
        set
        {
            _mesh = value;
            idx = mesh.triangles;
            pos = new List<Vector4>();
            mesh.GetUVs(1, pos);
            baryFace = new mat4x3[idx.Length / 3];
            baryEdge = new mat4x2[idx.Length];
            for (int i = 0; i < idx.Length; i += 3)
            {
                mat3x4 u = new mat3x4((R4)pos[i], (R4)pos[i + 1], (R4)pos[i + 2]);
                mat2x4[] us = new[] {new mat2x4((R4)pos[i    ], (R4)pos[i + 1]),
                                     new mat2x4((R4)pos[i + 1], (R4)pos[i + 2]),
                                     new mat2x4((R4)pos[i + 2], (R4)pos[i    ])};
                baryFace[i / 3] = (u.Transposed * u).Inverse * u.Transposed;
                baryEdge[i    ] = (us[0].Transposed * us[0]).Inverse * us[0].Transposed;
                baryEdge[i + 1] = (us[1].Transposed * us[1]).Inverse * us[1].Transposed;
                baryEdge[i + 2] = (us[2].Transposed * us[2]).Inverse * us[2].Transposed;
            }
        }
    }

    void Start()
    {
        mesh = new Mesh();
    }

    public override bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance)
    {
        hitInfo = new RaycastHitSpherical();
        hitInfo.collider = this;
        hitInfo.distance = Mathf.Infinity;

        for (int i = 0; i < idx.Length; i += 3)
        {
            float[] coeffs = Rot4.CoCross(pos[i], pos[i + 1], pos[i + 2], ray.org, ray.dir);

            if (coeffs[0] < 0 && coeffs[1] < 0 && coeffs[2] < 0) // if the ray hits the front of the triangle
            {
                Debug.Log("hit");
                float distance = Mathf.Atan2(coeffs[4], coeffs[3]);
                distance = distance < 0 ? distance + 2 * Mathf.PI : distance; // wrap into range 0 .. 2pi
                
                if (distance < hitInfo.distance) // if this is the new best
                {
                    hitInfo.distance = distance;
                    hitInfo.triangleIndex = i / 3;
                    hitInfo.point = (coeffs[3] * ray.org + coeffs[4] * ray.dir).normalized;
                }
            }
        }

        return hitInfo.distance <= maxDistance;
    }

    // TODO: this implementation checks every edge up to twice and every vertex up to once per face.
    // this is naive and can be improved with a specialized data structure.
    public override float ClosestPoint(Vector4 query, out Vector4 point)
    {
        float maxCosDist = -Mathf.Infinity;
        point = new Vector4();
        // check each triangle, and if there is not a potential nearest point in the triangle check it's edges
        for (int i = 0; i < idx.Length; i += 3)
        {
            vec3 baryCoords = baryFace[i / 3] * (R4)query;
            if ((baryCoords >= vec3.Zero).All)
            {
                Vector4 p = (baryCoords.x * pos[idx[i]] + baryCoords.y * pos[idx[i + 1]] + baryCoords.z * pos[idx[i + 2]]).normalized;
                float cosDist = Vector4.Dot(p, query);
                if (cosDist > maxCosDist)
                {
                    maxCosDist = cosDist;
                    point = p;
                }
            }
            // if there is no candidate point in the triangle, only then do we check the edges
            else
            {
                vec2[] baryPairs = {baryEdge[i] * (R4)query, baryEdge[i + 1] * (R4)query, baryEdge[i + 2] * (R4)query};
                // theoretically only one of these if statements will be called
                // this code could be written more concisely but it was all copy and paste anyways
                if ((baryPairs[0] >= vec2.Zero).All)
                {
                    Vector4 p = (baryPairs[0].x * pos[idx[i]] + baryPairs[0].y * pos[idx[i + 1]]).normalized;
                    float cosDist = Vector4.Dot(p, query);
                    if (cosDist > maxCosDist)
                    {
                        maxCosDist = cosDist;
                        point = p;
                    }
                }
                if ((baryPairs[1] >= vec2.Zero).All)
                {
                    Vector4 p = (baryPairs[1].x * pos[idx[i + 1]] + baryPairs[1].y * pos[idx[i + 2]]).normalized;
                    float cosDist = Vector4.Dot(p, query);
                    if (cosDist > maxCosDist)
                    {
                        maxCosDist = cosDist;
                        point = p;
                    }
                }
                if ((baryPairs[2] >= vec2.Zero).All)
                {
                    Vector4 p = (baryPairs[2].x * pos[idx[i + 2]] + baryPairs[2].y * pos[idx[i]]).normalized;
                    float cosDist = Vector4.Dot(p, query);
                    if (cosDist > maxCosDist)
                    {
                        maxCosDist = cosDist;
                        point = p;
                    }
                }
                if (baryPairs[2].x < 0 && baryPairs[0].y < 0)
                {
                    float cosDist = Vector4.Dot(pos[idx[i]], query);
                    if (cosDist > maxCosDist)
                    {
                        maxCosDist = cosDist;
                        point = pos[idx[i]];
                    }
                }
                if (baryPairs[0].x < 0 && baryPairs[1].y < 0)
                {
                    float cosDist = Vector4.Dot(pos[idx[i + 1]], query);
                    if (cosDist > maxCosDist)
                    {
                        maxCosDist = cosDist;
                        point = pos[idx[i + 1]];
                    }
                }
                if (baryPairs[1].x < 0 && baryPairs[2].y < 0)
                {
                    float cosDist = Vector4.Dot(pos[idx[i + 2]], query);
                    if (cosDist > maxCosDist)
                    {
                        maxCosDist = cosDist;
                        point = pos[idx[i + 2]];
                    }
                }
            }
        }

        return Mathf.Acos(maxCosDist);
    }
}
