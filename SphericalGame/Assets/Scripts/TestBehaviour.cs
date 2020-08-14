using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

public class TestBehaviour : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.SetTriangles(new[] { 0, 1, 2 }, 0);
        mesh.SetVertices(new[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0)
        });
        mesh.SetUVs(1, new[]
        {
            new Vector4(0, 0, -1, 0),
            new Vector4(0, -1, 0, 0),
            new Vector4(1, 0, 0, 0)
        });
        mesh.SetUVs(2, new[]
        {
            new Vector4(0, 0, 0, 1),
            new Vector4(0, 0, 0, 1),
            new Vector4(0, 0, 0, 1)
        });
    }

    public void Test()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        //List<Vector2> uvs = new List<Vector2>();
        //List<Vector4> pos = new List<Vector4>();
        //List<Vector4> nor = new List<Vector4>();
        //List<int> idx = new List<int>();
        //mf.mesh.GetUVs(0, uvs);
        //mf.mesh.GetUVs(1, pos);
        //mf.mesh.GetUVs(2, nor);
        //mf.mesh.GetTriangles(idx, 0);
        //Debug.Log(pos.Count);
        //Debug.Log(nor.Count);
        //Debug.Log(idx.Count);
        //for (int i = 0; i < 10; i++)
        //{
        //    Debug.Log(uvs[i]);
        //    Debug.Log(pos[i]);
        //    Debug.Log(nor[i]);
        //}
        //Debug.Log(mf.mesh.GetTopology(0));
        //foreach (VertexAttributeDescriptor att in mf.mesh.GetVertexAttributes())
        //{
        //    Debug.Log(att);
        //}

        Debug.Log(GetComponent<Renderer>().isVisible);

        //Terrain ter = GetComponent<Terrain>();
        //Debug.Log(ter.faces[0].center);
        //foreach (int i in ter.faces[0].mverts)
        //{
        //    Debug.Log(ter.verts[i]);
        //}
    }

    void OnDestroy()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Destroy(mf.mesh);
    }
}
