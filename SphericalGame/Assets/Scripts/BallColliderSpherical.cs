using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallColliderSpherical : ColliderSpherical
{
    [SerializeField]
    public Vector4 localCenter = R4.origin;
    public Vector4 center => trans.localToWorld * localCenter;
    public float cosRad;
    public float radius
    {
        get => Mathf.Acos(cosRad);
        set => cosRad = Mathf.Cos(value);
    }
    public TransformSpherical trans;

    void Start()
    {
        trans = GetComponent<TransformSpherical>();
        localCenter.Normalize();
    }

    // this is untested
    public override bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance)
    {
        hitInfo = new RaycastHitSpherical();
        hitInfo.collider = this;
        float a = Vector4.Dot(ray.org, center);
        float b = Vector4.Dot(ray.dir, center);
        float d = a * a + b * b - cosRad * cosRad;
        if (d < 0) // check discriminant
        {
            return false;
        }
        d = Mathf.Sqrt(d);
        // two solutions, xi is the coefficient of ray.org, yi is the coefficient of ray.dir
        float x1 = a * cosRad + b * d;
        float y1 = b * cosRad - a * d;
        float x2 = a * cosRad - b * d;
        float y2 = b * cosRad + a * d;
        float dist1 = Mathf.Atan2(y1, x1);
        float dist2 = Mathf.Atan2(y2, x2);
        dist1 = dist1 < 0 ? dist1 + 2 * Mathf.PI : dist1; // wrap into range 0 .. 2pi
        dist2 = dist2 < 0 ? dist2 + 2 * Mathf.PI : dist2;
        if (dist1 < dist2 ^ a > cosRad) // if first point is closer xor the origin is inside the ball, so the ray only hits the outside
        {
            hitInfo.distance = dist1;
            hitInfo.point = (x1 * ray.org + y1 * ray.dir).normalized;
        }
        else
        {
            hitInfo.distance = dist2;
            hitInfo.point = (x2 * ray.org + y2 * ray.dir).normalized;
        }
        return hitInfo.distance <= maxDistance;
    }

    public override float ClosestPoint(Vector4 query, out Vector4 point)
    {
        float a = Vector4.Dot(query, center);
        float d = Mathf.Sqrt(a * a + 1 - cosRad * cosRad);
        point = ((a * cosRad + d) * query + (cosRad - a * d) * center).normalized;
        return Mathf.Acos(Vector4.Dot(query, point));
    }
}
