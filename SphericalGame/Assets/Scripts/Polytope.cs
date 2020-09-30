using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

public enum Solid : byte
{
    Empty = 255,
    White = 0,
    Black = 1,
    Red = 2,
    Yellow = 3,
    Blue = 4
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
    public Vector4 center;
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
    public Vector4 center;
}

public class Polytope : MonoBehaviour
{
    public string topeFile = "Polytopes/oriented4800cell.tope";
    public int generateMode = 0;

    public bool updateMeshFlag;

    // mesh data
    private int inext;
    private List<Vector2> uvs;
    private List<Vector4> pos;
    private List<Vector4> nor;
    private List<int>[] idx = new List<int>[Enum.GetNames(typeof(Solid)).Length - 1];

    // collider mesh data
    private int inextc;
    private List<Vector4> posc;
    private List<int> idxc;
    public List<Face> triangleKey; // gives face cooresponding to each triangle index in collider mesh

    // polytope data
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

        System.IO.StreamReader file = new System.IO.StreamReader(Path.Combine(Application.streamingAssetsPath, topeFile));
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
        foreach (Cell c in cells)
        {
            c.center = Rot4.Center(Splice<Vector4>(verts, c.mverts).ToArray());
        }

        Generate();
    }

    void Update()
    {
        if (updateMeshFlag)
        {
            // mesh will be formatted so that uv channel 1 is position
            // and uv channel 2 is the orthogonal vector that the normal is facing
            updateMeshFlag = false;

            inext = 0;
            uvs = new List<Vector2>();
            pos = new List<Vector4>();
            nor = new List<Vector4>();
            for (int i = 0; i < idx.Length; i++)
            {
                idx[i] = new List<int>();
            }

            inextc = 0;
            posc = new List<Vector4>();
            idxc = new List<int>();
            triangleKey = new List<Face>();

            // draw each edge
            foreach (Edge e in edges)
            {
                if (e.contents == Solid.Empty) { continue; }

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

                    AddFaceRefined(new List<Vector4> { (R4)Quaternion.Slerp((R4)v0, (R4)f0.center, 1f/16f),
                                                       (R4)Quaternion.Slerp((R4)v1, (R4)f0.center, 1f/16f),
                                                       (R4)Quaternion.Slerp((R4)v1, (R4)f1.center, 1f/16f),
                                                       (R4)Quaternion.Slerp((R4)v0, (R4)f1.center, 1f/16f) }, 5, (int)e.contents);
                }
            }

            // draw each visible face
            foreach (Face f in faces)
            {
                Cell c0 = cells[f.mcells[0]];
                Cell c1 = cells[f.mcells[1]];

                if (c0.contents == Solid.Empty && c1.contents != Solid.Empty)
                {
                    List<Vector4> vs = Splice<Vector4>(verts, f.mverts);
                    AddFaceSimple(vs, f);
                    AddFaceRefined(vs, 1, (int)c1.contents); // this function modifies input, be careful TODO: fix this
                }
                else if (c1.contents == Solid.Empty && c0.contents != Solid.Empty)
                {
                    List<Vector4> vs = Splice<Vector4>(verts, f.mverts);
                    vs.Reverse();
                    AddFaceSimple(vs, f);
                    AddFaceRefined(vs, 1, (int)c0.contents);
                }
            }

            // TODO: Mesh requires vertices, this is my hacky solution. This should be fixed.
            Vector3[] zeroes = new Vector3[inext];
            Vector3[] zeroesc = new Vector3[inextc];
            int[] temp = new int[0];
            zeroes.Initialize();
            zeroesc.Initialize();

            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // TODO: it's probably better to use base vertices instead, or turn off refined mesh
            mesh.subMeshCount = Enum.GetNames(typeof(Solid)).Length - 1;
            for (int i = 0; i < idx.Length; i++)
            {
                mesh.SetTriangles(temp, i);
            }
            mesh.SetVertices(zeroes);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(1, pos);
            mesh.SetUVs(2, nor);
            for (int i = 0; i < idx.Length; i++)
            {
                mesh.SetTriangles(idx[i], i);
            }

            Mesh meshc = new Mesh();
            meshc.SetTriangles(temp, 0);
            meshc.SetVertices(zeroesc);
            meshc.SetUVs(1, posc);
            meshc.SetTriangles(idxc, 0);
            // we have to assign like this because the MeshColliderSpherical.mesh property has some additional set functionality
            GetComponent<MeshColliderSpherical>().mesh = meshc;
        }
    }

    public void Clicked(bool left, bool right, int triangleIndex, Solid solidType)
    {
        foreach (Cell c in Splice<Cell>(cells, triangleKey[triangleIndex].mcells))
        {
            if (left && c.contents != Solid.Empty)
            {
                c.contents = Solid.Empty;
                updateMeshFlag = true;
            }
            else if (right && c.contents == Solid.Empty)
            {
                c.contents = solidType;
                updateMeshFlag = true;
            }
        }
    }

    public void Generate()
    {
        if (generateMode == 0)
        {
            foreach (Edge e in edges)
            {
                e.contents = Solid.Empty;
            }
            foreach (Cell c in cells)
            {
                if (c.mverts.Contains(0))
                {
                    c.contents = Solid.White;
                }
                else
                {
                    c.contents = Solid.Empty;
                }
            }
        }

        else if (generateMode == 1)
        {
            foreach (Edge e in edges)
            {
                e.contents = Solid.Empty;
            }
            foreach (Cell c in cells)
            {
                c.contents = Solid.Empty;
                foreach (int i in c.mverts)
                {
                    if (verts[i].w < -0.05)
                    {
                        if (verts[i].x < 0)
                        {
                            c.contents = Solid.Blue;
                        }
                        else
                        {
                            c.contents = Solid.Yellow;
                        }
                    }
                }
            }
        }

        updateMeshFlag = true;
    }

    void OnDestroy()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Destroy(mf.mesh);
    }

    // adds a mesh face, not a polytope face
    // give vertices in clockwise order
    private void AddFace(List<Vector4> vs, int subMesh)
    {
        int n = vs.Count;
        for (int i = 0; i < n; i++)
        {
            uvs.Add(new Vector2((float)Mathf.Cos(i * 2 * Mathf.PI / n) * 0.5f + 0.5f,
                                (float)Mathf.Sin(i * 2 * Mathf.PI / n) * 0.5f + 0.5f));
        }

        pos.AddRange(vs);

        Vector4[] cross = new Vector4[n];
        cross[0] = Rot4.Cross(vs[0], vs[1], vs[2]).normalized;
        for (int i = 1; i < n; i++)
        {
            cross[i] = cross[0];
        }
        nor.AddRange(cross);

        for (int i = 2; i < n; i++)
        {
            idx[subMesh].AddRange(new[] { inext, inext + i - 1, inext + i });
        }
        inext += n;
    }

    // adds a face to the collider mesh, no normals
    // still give vertices in clockwise order
    private void AddFaceSimple(List<Vector4> vs, Face face)
    {
        posc.AddRange(vs);
        for (int i = 2; i < vs.Count; i++)
        {
            idxc.AddRange(new[] { inextc, inextc + i - 1, inextc + i });
            triangleKey.Add(face);
        }
        inextc += vs.Count;
    }

    // breaks a face into smaller triangles
    // WARNING: modifies input
    private void AddFaceRefined(List<Vector4> vs, int lod, int subMesh, int totalVertices = -1)
    {
        if (vs.Count < 3) { return; }
        int n = (lod + 2) * (lod + 1) / 2; // total # of new vertices
        if (totalVertices == -1) // in all child calls of this function totalVertices will remain the same
        {
            totalVertices = vs.Count;
        }

        Vector4[] cross = new Vector4[n];
        cross[0] = Rot4.Cross(vs[0], vs[1], vs[2]).normalized;
        for (int i = 1; i < n; i++)
        {
            cross[i] = cross[0];
        }
        nor.AddRange(cross);

        // uvs of the three main vertices in question
        Vector2 uv0 = new Vector2(1.0f, 0.5f);
        Vector2 uv1 = new Vector2((float)Mathf.Cos((1 - vs.Count) * 2 * Mathf.PI / totalVertices) * 0.5f + 0.5f,
                                  (float)Mathf.Sin((1 - vs.Count) * 2 * Mathf.PI / totalVertices) * 0.5f + 0.5f);
        Vector2 uv2 = new Vector2((float)Mathf.Cos((2 - vs.Count) * 2 * Mathf.PI / totalVertices) * 0.5f + 0.5f,
                                  (float)Mathf.Sin((2 - vs.Count) * 2 * Mathf.PI / totalVertices) * 0.5f + 0.5f);

        // square for loop iterates over rows of increasing length
        for (int i = 0; i <= lod; i++)
        {
            inext += i;

            // the ends of the current row
            Quaternion start = Quaternion.Slerp((R4)vs[1], (R4)vs[0], i / (float)lod);
            Quaternion end = Quaternion.Slerp((R4)vs[1], (R4)vs[2], i / (float)lod);
            Vector2 startUV = Vector2.Lerp(uv1, uv0, i / (float)lod);
            Vector2 endUV = Vector2.Lerp(uv1, uv2, i / (float)lod);

            for (int j = 0; j <= i; j++)
            {
                // numerical instability when start == end
                if (i > 0)
                {
                    pos.Add((R4)Quaternion.Slerp(start, end, j / (float)i));
                    uvs.Add(Vector2.Lerp(startUV, endUV, j / (float)i));
                }
                else
                {
                    pos.Add(vs[1]);
                    uvs.Add(uv1);
                }

                // two orientations of triangles
                int cur = inext + j;
                if (j < i)
                {
                    idx[subMesh].AddRange(new[] { cur, cur - i, cur + 1 });
                }
                if (j > 0 && j < i)
                {
                    idx[subMesh].AddRange(new[] { cur, cur - i - 1, cur - i });
                }
            }
        }
        inext += lod + 1;

        vs.RemoveAt(1);
        AddFaceRefined(vs, lod, subMesh, totalVertices);
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

    public List<T> Splice<T>(List<T> lis, List<int> ids)
    {
        List<T> o = new List<T>();
        foreach (int i in ids)
        {
            o.Add(lis[i]);
        }
        return o;
    }
}
