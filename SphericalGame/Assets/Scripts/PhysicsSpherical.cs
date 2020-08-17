using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsSpherical
{
    public static List<ColliderSpherical> colliders = new List<ColliderSpherical>();

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
}

public abstract class ColliderSpherical : MonoBehaviour
{
    public abstract bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance);

    void OnEnable()
    {
        PhysicsSpherical.colliders.Add(this);
    }

    void OnDisable()
    {
        PhysicsSpherical.colliders.Remove(this);
    }
}