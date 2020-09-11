using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using GlmSharp;

public class TestBehaviour : MonoBehaviour
{
    void Start()
    {
    }

    public void Test()
    {
        Polytope p = GameObject.Find("600Cell").GetComponent<Polytope>();
        Debug.Log(Vector4.Dot(p.faces[0].center, p.cells[p.faces[0].mcells[0]].center));
    }

    void FixedUpdate()
    {
        GameObject player = GameObject.Find("Main Camera");
        GameObject wall = GameObject.Find("600Cell");
        Vector4 p;
        wall.GetComponent<MeshColliderSpherical>().ClosestPoint(player.GetComponent<TransformSpherical>().position, out p);
        GetComponent<TransformSpherical>().localToWorld = Rot4.StraightTo((R4)p);
    }

    void OnDestroy()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Destroy(mf.mesh);
    }
}
