using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum Solid : byte
{
    Empty = 0,
    White,
    Black,
    Red,
    Blue
}

public class Edge
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

public class Face
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

public class Cell
{
    public Cell(List<int> iverts)
    {
        mverts = iverts;
        contents = Solid.Empty;
    }

    public List<int> mverts;
    public Solid contents;
}

public class Polytope : MonoBehaviour
{
    public bool updateMeshFlag;
    public int inext;
    public List<Vector4> pos;
    public List<Vector4> nor;
    public List<int> idx;
    
    public List<Vector4> verts;
    public List<Edge> edges;
    public List<Face> faces;
    public List<Cell> cells;

    void Start()
    {
        updateMeshFlag = true;
        verts = new List<Vector4>();
        edges = new List<Edge>();
        faces = new List<Face>();
        cells = new List<Cell>();

        System.IO.StreamReader file = new System.IO.StreamReader(@"Assets/Polytopes/oriented600cell.tope");
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

                    AddFaceRefined(new List<Vector4> { (R4)Quaternion.Slerp((R4)v0, (R4)f0.center, 1f/8f),
                                                       (R4)Quaternion.Slerp((R4)v1, (R4)f0.center, 1f/8f),
                                                       (R4)Quaternion.Slerp((R4)v1, (R4)f1.center, 1f/8f),
                                                       (R4)Quaternion.Slerp((R4)v0, (R4)f1.center, 1f/8f) }, 5);
                }
            }

            // TODO: Mesh requires vertices, this is my hacky solution. This should be fixed.
            Vector3[] zeroes = new Vector3[inext];
            zeroes.Initialize();

            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // TODO: it's probably better to use base vertices instead, or turn off refined mesh
            mesh.SetVertices(zeroes);
            mesh.SetUVs(1, pos);
            mesh.SetUVs(2, nor);
            mesh.SetTriangles(idx, 0);
        }
    }

    void OnDestroy()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Destroy(mf.mesh);
    }

    // adds a mesh face, not a polytope face
    // give vertices in clockwise order
    private void AddFace(List<Vector4> vs)
    {
        pos.AddRange(vs);

        Vector4[] cross = new Vector4[vs.Count];
        cross[0] = Rot4.Cross(vs[0], vs[1], vs[2]).normalized;
        for (int i = 1; i < vs.Count; i++)
        {
            cross[i] = cross[0];
        }
        nor.AddRange(cross);

        for (int i = 2; i < vs.Count; i++)
        {
            idx.AddRange(new[] { inext, inext + i - 1, inext + i });
        }
        inext += vs.Count;
    }

    // breaks a face into smaller triangles
    private void AddFaceRefined(List<Vector4> vs, int lod)
    {
        if (vs.Count < 3) { return; }
        int n = (lod + 2) * (lod + 1) / 2; // total # of new vertices

        Vector4[] cross = new Vector4[n];
        cross[0] = Rot4.Cross(vs[0], vs[1], vs[2]).normalized;
        for (int i = 1; i < n; i++)
        {
            cross[i] = cross[0];
        }
        nor.AddRange(cross);

        // square for loop iterates over rows of increasing length
        for (int i = 0; i <= lod; i++)
        {
            inext += i;

            // the ends of the current row
            Quaternion start = Quaternion.Slerp((R4)vs[1], (R4)vs[0], i / (float)lod);
            Quaternion end = Quaternion.Slerp((R4)vs[1], (R4)vs[2], i / (float)lod);

            for (int j = 0; j <= i; j++)
            {
                // numerical instability when start == end
                if (i > 0)
                {
                    pos.Add((R4)Quaternion.Slerp(start, end, j / (float)i));
                }
                else
                {
                    pos.Add(vs[1]);
                }

                // two orientations of triangles
                int cur = inext + j;
                if (j < i)
                {
                    idx.AddRange(new[] { cur, cur - i, cur + 1 });
                }
                if (j > 0 && j < i)
                {
                    idx.AddRange(new[] { cur, cur - i - 1, cur - i });
                }
            }
        }
        inext += lod + 1;

        vs.RemoveAt(1);
        AddFaceRefined(vs, lod);
    }

    Vector4 ParseCoords(string s)
    {
        string[] l = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
        foreach (string s in l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            o.Add(int.Parse(s));
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
