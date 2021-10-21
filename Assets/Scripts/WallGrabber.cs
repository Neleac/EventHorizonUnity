using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGrabber : MonoBehaviour
{
    public OVRInput.Controller controller;

    [HideInInspector] public bool wallGrab;
    [HideInInspector] public int collidersInWall;
    [HideInInspector] public int collidersInLadder;
    [HideInInspector] public float prevY;

    void Start()
    {
        wallGrab = false;
        collidersInWall = 0;
        collidersInLadder = 0;
        prevY = transform.position.y;
    }

    void LateUpdate()
    {
        // update hand position for climbing ladder
        if (Physics.gravity != Vector3.zero)
            prevY = transform.position.y;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Wall")
            collidersInWall++;

        if (collider.transform.parent != null && collider.transform.parent.name == "Ladder")
            collidersInLadder++;
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Wall")
            collidersInWall--;

        if (collider.transform.parent != null && collider.transform.parent.name == "Ladder")
            collidersInLadder--;
    }
}
