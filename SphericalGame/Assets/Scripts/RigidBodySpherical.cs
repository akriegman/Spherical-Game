using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodySpherical : MonoBehaviour
{
    public bool gravity = true; // affected by gravity
    public bool physical = true; // receives forces
    public bool oriented = false; // fixed orientation relative to gravity

    private Vector4 vel = R4.zero;
    public List<BallColliderSpherical> colls;
    public TransformSpherical trans;

    // moves in local space
    // see Rot4.FromTangent() for info on the meaning of move and turn
    public void MoveLocal(Vector3 move, Vector3 turn)
    {

    }

    // inputs should be in tangent space to rigidbody's position
    // with the origin in R^4 being the point of tangency
    // the rotation is a right handed rotation around turn
    public void MoveGlobal(Vector4 move, Vector4 turn)
    {

    }

    void FixedUpdate()
    {
        if (oriented)
        {
            Quaternion sky = trans.worldToLocal * ((Quaternion)Globals.sky * (R4)trans.position);
            Vector3 sky3 = new Vector3(sky.x, sky.y, sky.z);
            Vector3 newLeft = Vector3.Cross(sky3, Vector3.forward);
            if (newLeft.sqrMagnitude > Mathf.Epsilon)
            {
                // Vector3.right is actually left because we use a different orientation
                Quaternion correctionRotation = Quaternion.FromToRotation(Vector3.right, newLeft);
                trans.localToWorld = trans.localToWorld * new Rot4(correctionRotation, Quaternion.Inverse(correctionRotation));
            }
        }
    }

    void Start()
    {
        colls = new List<BallColliderSpherical>(GetComponents<BallColliderSpherical>());
        trans = GetComponent<TransformSpherical>();
    }

    void OnEnable()
    {
        PhysicsSpherical.rigidBodies.Add(this);
    }

    void OnDisable()
    {
        PhysicsSpherical.rigidBodies.Remove(this);
    }
}
