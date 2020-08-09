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
using UnityEngine;

public class Rot4
{
    Quaternion ml, mr;

    public static Rot4 identity => new Rot4(Quaternion.identity, Quaternion.identity);

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
        Quaternion root = Quaternion.Slerp(Quaternion.identity, q, 0.5f);
        return new Rot4(root, root);
    }

    // move components are 1->i, 1->j, 1->k
    // turn components are j->k, k->i, i->j
    public static Rot4 FromTangent(Vector3 move, Vector3 turn, float dt)
    {
        if (dt > 0.1)
        {
            dt = 0.1f;
        }
        Vector3 left = (move + turn) / 2f * dt;
        Vector3 right = (move - turn) / 2f * dt;
        return new Rot4(new Quaternion(left.x, left.y, left.z, Mathf.Sqrt(1 - left.sqrMagnitude)),
                        new Quaternion(right.x, right.y, right.z, Mathf.Sqrt(1 - right.sqrMagnitude)));
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
    // named cross because it's kinda like a cross product
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

    public static implicit operator Quaternion(R4 r) => new Quaternion(r.d.x, r.d.y, r.d.z, r.d.w);
    public static implicit operator Vector4(R4 r) => r.d;
    public static implicit operator R4(Quaternion q) => new R4(q);
    public static implicit operator R4(Vector4 v) => new R4(v);
}
