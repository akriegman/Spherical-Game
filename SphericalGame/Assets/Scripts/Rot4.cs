// The linear algebra library for my spherical game
// Written by Aaron Kriegman
// A note on orientation:
// The standard right hand rule applies and the game as it appears on the screen should follow the right hand rule
// So, if you wrap your fingers through 1 to i, then your thumb should point from j to k because this cooresponds to
// a left multiplication by i. We will also have the convention that the order of the axes is i j k 1, so the hypervolume
// spanned by the vector i, j, k, 1 in that order has posititve volume. I think this is compatible with the previous
// convention and that two conentions are necessary because one orients 4-space and the other determines which "side"
// to view the 3-sphere from.

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GlmSharp;

public class Rot4
{
    public Quaternion ml, mr;

    public static Rot4 identity => new Rot4(Quaternion.identity, Quaternion.identity);
    public Rot4()
    {
        ml = Quaternion.identity; mr = Quaternion.identity;
    }
    public Rot4(Quaternion l, Quaternion r)
    {
        ml = l; mr = r;
    }

    public Rot4 Inverse()
    {
        return new Rot4(Quaternion.Inverse(ml), Quaternion.Inverse(mr));
    }

    public Matrix4x4 Matrix()
    {
        return QuatToLMat(ml) * QuatToRMat(mr);
    }

    public static Rot4 StraightTo(Quaternion q)
    {
        Vector4 root = (R4)q + R4.origin;
        float len = root.magnitude;
        root = len < Mathf.Epsilon ? R4.i : root / len;
        return new Rot4((R4)root, (R4)root);
    }

    // I hypothesise this works
    public static Rot4 StraightFromTo(Quaternion from, Quaternion to)
    {
        Rot4 o = StraightTo(Quaternion.Inverse(from) * to);
        o.ml = from * o.ml * Quaternion.Inverse(from);
        return o;
    }

    // move components are 1->i, 1->j, 1->k
    // turn components are j->k, k->i, i->j
    public static Rot4 FromTangentStereographic(Vector3 move, Vector3 turn, float dt)
    {
        Vector4 left = new R4((move + turn) / 2f * dt, 1f);
        Vector4 right = new R4((move - turn) / 2f * dt, 1f);
        return new Rot4((R4)left.normalized, (R4)right.normalized);
    }

    public static Rot4 FromTangentExponential(Vector3 move, Vector3 turn, float dt)
    {
        Vector3 left = (move + turn) / 2f * dt;
        Vector3 right = (move - turn) / 2f * dt;
        return new Rot4(Rot4.Exp(left), Rot4.Exp(right));
    }

    // exponential function for quaternions
    // input is the vector part of a pure imaginary quaternion
    public static R4 Exp(Vector3 x)
    {
        float theta = x.magnitude;
        if (theta < Mathf.Epsilon)
        {
            return R4.origin;
        }
        return new R4(x / theta * Mathf.Sin(theta), Mathf.Cos(theta));
    }

    // returns the center of a set of quaternions
    // takes linear average and normalizes, don't expect this to work over large distances
    public static Vector4 Center(Vector4[] vs)
    {
        Vector4 sum = Vector4.zero;
        foreach (Vector4 v in vs)
        {
            sum += v;
        }
        return sum.normalized;
    }

    // returns the vector 90 degrees away from all three inputs
    // oriented so that the volume t u v o is positive
    // so you should pass t u v in clockwise order
    // named cross because it's the ternary cross product
    public static Vector4 Cross(Vector4 t, Vector4 u, Vector4 v)
    {
        // taking minors, ti uj - tj ui
        float xy = t.x * u.y - t.y * u.x;
        float xz = t.x * u.z - t.z * u.x;
        float xw = t.x * u.w - t.w * u.x;
        float yz = t.y * u.z - t.z * u.y;
        float yw = t.y * u.w - t.w * u.y;
        float zw = t.z * u.w - t.w * u.z;
        return new Vector4(- yz*v.w - zw*v.y + yw*v.z,
                             xz*v.w + zw*v.x - xw*v.z,
                           - xy*v.w - yw*v.x + xw*v.y,
                             xy*v.z + yz*v.x - xz*v.y);
    }

    // the co cross product takes a rank 4 set of 5 vectors in R^4 and returns
    // the coefficients of a nontrivial linear combination of the vectors that comes out to 0
    // it's called CoCross because it's sort of the dual of the cross product
    // I'm not sure how to explain the orientation convention, try seeing how this is used
    // in MeshColliderSpherical.Raycast() if confused
    public static float[] CoCross(params Vector4[] v)
    {
        // 2x2 minors
        float ab = v[0][0] * v[1][1] - v[1][0] * v[0][1];
        float ac = v[0][0] * v[2][1] - v[2][0] * v[0][1];
        float ad = v[0][0] * v[3][1] - v[3][0] * v[0][1];
        float ae = v[0][0] * v[4][1] - v[4][0] * v[0][1];
        float bc = v[1][0] * v[2][1] - v[2][0] * v[1][1];
        float bd = v[1][0] * v[3][1] - v[3][0] * v[1][1];
        float be = v[1][0] * v[4][1] - v[4][0] * v[1][1];
        float cd = v[2][0] * v[3][1] - v[3][0] * v[2][1];
        float ce = v[2][0] * v[4][1] - v[4][0] * v[2][1];
        float de = v[3][0] * v[4][1] - v[4][0] * v[3][1];
        // 3x3 minors
        float abc = v[0][2] * bc - v[1][2] * ac + v[2][2] * ab;
        float abd = v[0][2] * bd - v[1][2] * ad + v[3][2] * ab;
        float abe = v[0][2] * be - v[1][2] * ae + v[4][2] * ab;
        float acd = v[0][2] * cd - v[2][2] * ad + v[3][2] * ac;
        float ace = v[0][2] * ce - v[2][2] * ae + v[4][2] * ac;
        float ade = v[0][2] * de - v[3][2] * ae + v[4][2] * ad;
        float bcd = v[1][2] * cd - v[2][2] * bd + v[3][2] * bc;
        float bce = v[1][2] * ce - v[2][2] * be + v[4][2] * bc;
        float bde = v[1][2] * de - v[3][2] * be + v[4][2] * bd;
        float cde = v[2][2] * de - v[3][2] * ce + v[4][2] * cd;
        // wow that was not bad at all
        return new float[] { -v[1][3] * cde + v[2][3] * bde - v[3][3] * bce + v[4][3] * bcd,
                              v[0][3] * cde - v[2][3] * ade + v[3][3] * ace - v[4][3] * acd,
                             -v[0][3] * bde + v[1][3] * ade - v[3][3] * abe + v[4][3] * abd,
                              v[0][3] * bce - v[1][3] * ace + v[2][3] * abe - v[4][3] * abc,
                             -v[0][3] * bcd + v[1][3] * acd - v[2][3] * abd + v[3][3] * abc };
    }

    // matrix representing left multiplication
    public static Matrix4x4 QuatToLMat(Quaternion q)
    {
        // these are the columns
        return new Matrix4x4(new Vector4( q.w, q.z,-q.y,-q.x),
                             new Vector4(-q.z, q.w, q.x,-q.y),
                             new Vector4( q.y,-q.x, q.w,-q.z),
                             new Vector4( q.x, q.y, q.z, q.w));
    }

    // matrix representing right multiplication
    public static Matrix4x4 QuatToRMat(Quaternion q)
    {
        // these are the columns
        return new Matrix4x4(new Vector4( q.w,-q.z, q.y,-q.x),
                             new Vector4( q.z, q.w,-q.x,-q.y),
                             new Vector4(-q.y, q.x, q.w,-q.z),
                             new Vector4( q.x, q.y, q.z, q.w));
    }

    public static Vector4 QuatToVec(Quaternion q)
    {
        return new Vector4(q.x, q.y, q.z, q.w);
    }

    public static Quaternion VecToQuat(Vector4 v)
    {
        return new Quaternion(v.x, v.y, v.z, v.w);
    }

    public override string ToString()
    {
        return ml.ToString() + ", " + mr.ToString();
    }

    public static Quaternion operator *(Rot4 R, Quaternion q)
    {
        return R.ml * q * R.mr;
    }

    public static Vector4 operator *(Rot4 R, Vector4 v)
    {
        Quaternion prod = R.ml * new Quaternion(v.x, v.y, v.z, v.w) * R.mr;
        return new Vector4(prod.x, prod.y, prod.z, prod.w);
    }

    public static Rot4 operator *(Rot4 Q, Rot4 R)
    {
        return new Rot4(Q.ml * R.ml, R.mr * Q.mr);
    }
}

// A struct just for converting between Vector4 and Quaternion
public struct R4
{
    Vector4 d;

    public R4(Quaternion q)
    {
        d = new Vector4(q.x, q.y, q.z, q.w);
    }
    public R4(Vector4 v)
    {
        d = v;
    }
    public R4(vec4 v)
    {
        d = new Vector4(v.x, v.y, v.z, v.w);
    }
    public R4(Vector3 v, float f)
    {
        d = new Vector4(v.x, v.y, v.z, f);
    }
    public R4(float x, float y, float z, float w)
    {
        d = new Vector4(x, y, z, w);
    }

    public static implicit operator Quaternion(R4 r) => new Quaternion(r.d.x, r.d.y, r.d.z, r.d.w);
    public static implicit operator Vector4(R4 r) => r.d;
    public static implicit operator vec4(R4 r) => new vec4(r.d.x, r.d.y, r.d.z, r.d.w);
    public static implicit operator R4(Quaternion q) => new R4(q);
    public static implicit operator R4(Vector4 v) => new R4(v);
    public static implicit operator R4(vec4 v) => new R4(v);

    public static Vector4 i = new Vector4(1, 0, 0, 0);
    public static Vector4 j = new Vector4(0, 1, 0, 0);
    public static Vector4 k = new Vector4(0, 0, 1, 0);
    public static Vector4 look = k;
    public static Vector4 origin = new Vector4(0, 0, 0, 1);
    public static Vector4 up = j;
    public static Vector4 zero = new Vector4(0, 0, 0, 0);

    public override string ToString() => String.Format("({0:F3}, {1:F3}, {2:F3}, {3:F3})", d.x, d.y, d.z, d.w);
}