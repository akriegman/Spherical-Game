using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsSpherical
{
    
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

}

public abstract class ColliderSpherical
{
    public abstract bool Raycast(RaySpherical ray, out RaycastHitSpherical hitInfo, float maxDistance);
}