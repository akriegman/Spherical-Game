using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsSpherical
{
    public static List<ColliderSpherical> colliders = new List<ColliderSpherical>();
    public static List<RigidBodySpherical> rigidBodies = new List<RigidBodySpherical>();

    // not automatically called by the Unity engine
    // that's where the Supervisor comes in
    public static void FixedUpdate()
    {
        foreach (RigidBodySpherical rgd in rigidBodies)
        {
            foreach (BallColliderSpherical ball in rgd.colls)
            {
                foreach (ColliderSpherical col in colliders)
                {
                    // an object shouldn't collide with itself
                    if (col is BallColliderSpherical b && rgd.colls.Contains(b)) { continue; }

                    Vector4 closestPoint;
                    if (col.ClosestPoint(ball.center, out closestPoint) <= ball.radius)
                    {
                        Vector4 skinPoint;
                        ball.ClosestPoint(closestPoint, out skinPoint);
                        rgd.trans.localToWorld = Rot4.StraightFromTo((R4)skinPoint, (R4)closestPoint) * rgd.trans.localToWorld;
                    }
                }
            }
        }
    }

    public static bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance = Mathf.Infinity)
    {
        hitInfo = new RaycastHitSpherical();
        hitInfo.distance = Mathf.Infinity;
        RaycastHitSpherical tempInfo;
        foreach (ColliderSpherical col in colliders)
        {
            if (col.Raycast(ray, out tempInfo, maxDistance))
            {
                if (tempInfo.distance < hitInfo.distance)
                {
                    hitInfo = tempInfo;
                }
            }
        }
        return hitInfo.distance < maxDistance;
    }
}

public struct RaySpherical
{
    public Vector4 org;
    public Vector4 dir;

    public RaySpherical(Vector4 origin, Vector4 direction)
    {
        org = origin;
        dir = direction;
    }
}

public struct RaycastHitSpherical
{
    public ColliderSpherical collider;
    public float distance;
    public int triangleIndex;
    public Vector4 point;
}

public abstract class ColliderSpherical : MonoBehaviour
{
    public abstract bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance);
    public abstract float ClosestPoint(Vector4 query, out Vector4 point);

    void OnEnable()
    {
        PhysicsSpherical.colliders.Add(this);
    }

    void OnDisable()
    {
        PhysicsSpherical.colliders.Remove(this);
    }
}