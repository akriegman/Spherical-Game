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
