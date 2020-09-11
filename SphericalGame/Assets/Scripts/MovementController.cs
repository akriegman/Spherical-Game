using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovementController : MonoBehaviour
{
    private TransformSpherical trans;

    private float nextAction;

    public Solid solidType;
    private bool showEdges = false;

    void Start()
    {
        trans = GetComponent<TransformSpherical>();
        nextAction = 0;
        solidType = Solid.White;
    }

    void Update()
    {
        if ( (Input.GetMouseButton(0) || Input.GetMouseButton(1)) && Time.time > nextAction)
        {
            nextAction = Time.time + Globals.actionRate;
            RaycastHitSpherical hit;
            if (PhysicsSpherical.Raycast(trans.lookRay, out hit, Mathf.PI))
            {
                Polytope poly = hit.collider.GetComponent<Polytope>();
                if (poly != null)
                {
                    poly.Clicked(Input.GetMouseButton(0), Input.GetMouseButton(1), hit.triangleIndex, solidType);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            solidType = (Solid)0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            solidType = (Solid)1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            solidType = (Solid)2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            solidType = (Solid)3;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            solidType = (Solid)4;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Polytope poly = FindObjectOfType<Polytope>();
            showEdges = !showEdges;
            poly.updateMeshFlag = true;
            if (showEdges)
            {
                foreach (Edge e in poly.edges)
                {
                    e.contents = Solid.Black;
                }
            }
            else
            {
                foreach (Edge e in poly.edges)
                {
                    e.contents = Solid.Empty;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            SceneManager.LoadScene(2);
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SceneManager.LoadScene(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SceneManager.LoadScene(0);
        }
    }

    void FixedUpdate()
    {
        // Vector3 move = Vector3.ClampMagnitude(new Vector3(-Input.GetAxis("Horizontal"), Input.GetAxis("OtherVertical"), Input.GetAxis("Vertical")), 1f);
        Vector3 move = Vector3.ClampMagnitude(new Vector3((Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? -1 : 0),
                                                          (Input.GetKey(KeyCode.Space) ? 1 : 0) + (Input.GetKey(KeyCode.LeftShift) ? -1 : 0),
                                                          (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0)), 1);
        Vector3 turn = new Vector3(-8.5f * Input.GetAxis("Mouse Y"), -8.5f * Input.GetAxis("Mouse X"), 3 * Input.GetAxis("Spin"));
        trans.localToWorld = trans.localToWorld * Rot4.FromTangentStereographic(move, turn, Time.fixedDeltaTime);
    }
}
