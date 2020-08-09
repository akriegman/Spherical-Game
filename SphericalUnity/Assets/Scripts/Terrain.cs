using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

enum Solid : byte
{
    Empty = 0,
    White,
    Black,
    Red,
    Blue
}

class Edge
{
    public Edge(List<int> iverts, List<int> ifaces)
    {
        mverts = iverts;
        mfaces = ifaces;
        contents = Solid.White;
    }

    public List<int> mverts;
    public List<int> mfaces;
    public Solid contents;
}

class Face
{
    public Face(List<int> iverts, List<int> icells)
    {
        mverts = iverts;
        mcells = icells;
        center = new Vector4(0, 0, 0, 0);
    }

    public List<int> mverts;
    public List<int> mcells;
    public Vector4 center { get; set; }
}

class Cell
{
    public Cell(List<int> iverts)
    {
        mverts = iverts;
        contents = Solid.Empty;
    }

    public List<int> mverts;
    public Solid contents;
}

public class Terrain : MonoBehaviour
{
    public bool updateMeshFlag;
    private int inext;
    List<Vector4> pos;
    List<Vector4> nor;
    List<int> idx;

    List<Vector4> verts;
    List<Edge> edges;
    List<Face> faces;
    List<Cell> cells;

    void Start()
    {
        updateMeshFlag = true;
        verts = new List<Vector4>();
        edges = new List<Edge>();
        faces = new List<Face>();
        cells = new List<Cell>();

        System.IO.StreamReader file = new System.IO.StreamReader(@"../Polytopes/oriented600cell.tope");
        string line;
        string[] split;
        while ((line = file.ReadLine()) != null)
        {
            split = line.Split(':');
            switch (split[0])
            {
                case "#":
                    break;
                case "v":
                    verts.Add(ParseCoords(split[1]));
                    break;
                case "e":
                    edges.Add(new Edge(ParseIndices(split[1]), ParseIndices(split[2])));
                    break;
                case "f":
                    faces.Add(new Face(ParseIndices(split[1]), ParseIndices(split[2])));
                    break;
                case "c":
                    cells.Add(new Cell(ParseIndices(split[1])));
                    break;
            }
        }
        file.Close();

        // populate centers
        foreach (Face f in faces)
        {
            f.center = Rot4.Center(Splice<Vector4>(verts, f.mverts).ToArray());
        }
    }

    void Update()
    {
        if (updateMeshFlag)
        {
            // mesh will be formatted so that uv channel 1 is position
            // and uv channel 2 is the orthogonal vector that the normal is facing
            updateMeshFlag = false;
            inext = 0;
            pos = new List<Vector4>();
            nor = new List<Vector4>();
            idx = new List<int>();

            // draw each edge
            foreach (Edge e in edges)
            {
                // draw a panel of the edge between each pair of connected faces
                for (int i = 0; i < e.mfaces.Count; i++)
                {
                    // find relevant components
                    Vector4 v0 = verts[e.mverts[0]];
                    Vector4 v1 = verts[e.mverts[1]];
                    Face f0 = faces[e.mfaces[i]];
                    Face f1 = faces[e.mfaces[(i + 1) % e.mfaces.Count]];
                    Cell c = cells[f0.mcells.Intersect(f1.mcells).ToArray()[0]];
                    // skip this panel if it's not visible
                    if (c.contents != Solid.Empty) { continue; }

                    AddFace(new Vector4[] { (R4)Quaternion.Slerp((R4)v0, (R4)f0.center, 1f/16f),
                                            (R4)Quaternion.Slerp((R4)v1, (R4)f0.center, 1f/16f),
                                            (R4)Quaternion.Slerp((R4)v1, (R4)f1.center, 1f/16f),
                                            (R4)Quaternion.Slerp((R4)v0, (R4)f1.center, 1f/16f)});
                }
            }

            MeshFilter mf = GetComponent<MeshFilter>();
            mf.mesh.SetUVs(1, pos);
            mf.mesh.SetUVs(2, nor);
            mf.mesh.SetTriangles(idx, 0);
        }
    }

    // adds a mesh face, not a polytope face
    // give vertices in clockwise order
    private void AddFace(Vector4[] vs)
    {
        pos.AddRange(vs);
        Vector4[] cross = new Vector4[vs.Length];
        Array.Fill<Vector4>(cross, Rot4.Cross(vs[0], vs[1], vs[2]));
        nor.AddRange(cross);
        for (int i = 2; i < vs.Length; i++)
        {
            idx.AddRange(new[] { inext, inext + i - 1, inext + i });
        }
        inext += vs.Length;
    }

    Vector4 ParseCoords(string s)
    {
        string[] l = s.Split(' ');
        Vector4 o = new Vector4();
        for (int i = 0; i < 4; i++)
        {
            o[i] = float.Parse(l[i]);
        }
        return o;
    }

    List<int> ParseIndices(string l)
    {
        List<int> o = new List<int>();
        foreach (string s in l.Split(' '))
        {
            try
            {
                o.Add(int.Parse(s));
            }
            catch (FormatException) {}
        }
        return o;
    }

    List<T> Splice<T>(List<T> lis, List<int> ids)
    {
        List<T> o = new List<T>();
        foreach (int i in ids)
        {
            o.Add(lis[i]);
        }
        return o;
    }
}
