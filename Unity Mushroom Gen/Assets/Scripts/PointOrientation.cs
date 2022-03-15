using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public struct PointOrientation
{
    public Vector3 pos;
    public Quaternion rot;

    public PointOrientation(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }

    public PointOrientation(Vector3 pos, Vector3 forward)
    {
        this.pos = pos;
        this.rot = Quaternion.LookRotation(forward);
    }

    public Vector3 LocaltoWorld(Vector3 localSpacePos)
    {
       return pos + rot * localSpacePos;
    }
    
}
