using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProjector : MonoBehaviour
{
    public float radius = 3;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        List<Vector4> newPos = new List<Vector4>();
        List<Vector4> newNor = new List<Vector4>();

        // lol I just realized I wrote this using stereographic projection for normals instead of exponential projection
        // they're approximately the same for small models so we'll leave it for now
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = vertices[i];
            Vector3 nor = normals[i];
            float len = pos.magnitude;
            float s = Mathf.Sin(len / radius);
            Quaternion q = new Quaternion(pos.x * s / len, pos.y * s / len, pos.z * s / len, Mathf.Cos(len / radius)); // position in curved space
            Vector4 n = Rot4.StraightTo(q) * new Vector4(nor.x, nor.y, nor.z, 0f);
            //Quaternion p = new Quaternion(n.x, n.y, n.z, n.w);
            //Quaternion pqi = p * Quaternion.Inverse(q);
            //Quaternion l = Quaternion.LookRotation(new Vector3(pqi.x, pqi.y, pqi.z));
            //Quaternion r = Quaternion.Inverse(l) * q;
            newPos.Add((R4)q);
            newNor.Add(-n);
        }

        mesh.SetUVs(1, newPos);
        mesh.SetUVs(2, newNor);
    }

    void OnDestroy()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Destroy(mf.mesh);
    }
}
