using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProjector : MonoBehaviour
{
    public bool recenter = true;
    public float scale = 0.3f;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int[] indices = mesh.triangles;
        List<Vector4> newPos = new List<Vector4>();
        List<Vector4> newNor = new List<Vector4>();

        if (normals.Length < vertices.Length)
        {
            mesh.RecalculateNormals();
            normals = mesh.normals;
        }

        if (recenter)
        {
            Vector3 center = Vector3.zero;
            foreach (Vector3 v in vertices)
            {
                center += v;
            }
            center /= vertices.Length;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= center;
            }
        }

        // lol I just realized I wrote this using stereographic projection for normals instead of exponential projection
        // they're approximately the same for small models so we'll leave it for now
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = vertices[i];
            Vector3 nor = normals[i];
            float len = pos.magnitude;
            float s = Mathf.Sin(len / Globals.radius * scale);
            Quaternion q = new Quaternion(pos.x * s / len, pos.y * s / len, pos.z * s / len, Mathf.Cos(len / Globals.radius * scale)); // position in curved space
            Vector4 n = Rot4.StraightTo(q) * new Vector4(nor.x, nor.y, nor.z, 0f);
            //Quaternion p = new Quaternion(n.x, n.y, n.z, n.w);
            //Quaternion pqi = p * Quaternion.Inverse(q);
            //Quaternion l = Quaternion.LookRotation(new Vector3(pqi.x, pqi.y, pqi.z));
            //Quaternion r = Quaternion.Inverse(l) * q;
            newPos.Add((R4)q);
            newNor.Add(n);
        }

        // flip indices because normal video game space has opossite orientation of my space I think
        for (int i = 0; i < indices.Length; i += 3)
        {
            int temp = indices[i];
            indices[i] = indices[i + 1];
            indices[i + 1] = temp;
        }

        mesh.triangles = indices;
        mesh.SetUVs(1, newPos);
        mesh.SetUVs(2, newNor);
    }

    void OnDestroy()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Destroy(mf.mesh);
    }
}
