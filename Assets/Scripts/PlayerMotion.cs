using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerMotion : MonoBehaviour
{
    private XRRig rig;
    private CharacterController controller;
    private CapsuleCollider collider;
    private Rigidbody body;
    private Transform eyes;
    private WallGrabber leftWG;
    private WallGrabber rightWG;

    private Vector3 prevPos;                    // player previous position
    private Vector3 prevHandPos;                
    private Vector3 grabStartPos;               // hand position of wall grab start / ladder grab start
    private float grabStartTime;                // time of wall grab start
    private Vector3 direction;                  // push/pull move direction
    private float speed;                        // push/pull move speed
    private const float JETSPEED = 0.25f;       // jetpack speed constant
    private const float BRAKESPEED = 2f;        // brake speed constant

    void Start()
    {
        rig = GetComponent<XRRig>();
        controller = GetComponent<CharacterController>();
        collider = GetComponent<CapsuleCollider>();

        GameObject cameraOffset = GameObject.Find("OVRCameraRig/TrackingSpace");
        eyes = cameraOffset.transform.Find("CenterEyeAnchor");
        leftWG = cameraOffset.transform.Find("LeftHandAnchor/CustomHandLeft").GetComponent<WallGrabber>();
        rightWG = cameraOffset.transform.Find("RightHandAnchor/CustomHandRight").GetComponent<WallGrabber>();

        prevPos = transform.position;
    }

    void Update()
    {
        if (Physics.gravity == Vector3.zero)    // 0G movement
        {
            UpdateCapsuleCollider();

            // push/pull: move in hand motion opposite direction 
            if (wallGrabMove(leftWG))
                body.AddForce(direction * speed, ForceMode.VelocityChange);

            if (wallGrabMove(rightWG))
                body.AddForce(direction * speed, ForceMode.VelocityChange);

            // jetpack: move in gaze direction
            if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
                body.AddForce(eyes.forward * JETSPEED, ForceMode.VelocityChange);

            // stop move: grab wall, press right joystick
            if (leftWG.wallGrab || rightWG.wallGrab || OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))
                body.AddForce(-(transform.position - prevPos) * BRAKESPEED, ForceMode.VelocityChange);

            // record previous position to track motion
            prevPos = transform.position;
        }
        else    // ladder climbing
        {
            ladderMove(leftWG);
            ladderMove(rightWG);
            if (!leftWG.wallGrab && ! rightWG.wallGrab)
                controller.SimpleMove(Vector3.zero);
        }
    }

    public void InstantiateBody()
    {
        body = GetComponent<Rigidbody>();
    }

    private void ladderMove(WallGrabber wg)
    {
        if (wg.wallGrab)
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, wg.controller) > 0.9f)
            {
                float yDist = wg.transform.position.y - wg.prevY;
                controller.Move(Vector3.down * Time.deltaTime * yDist * 100);
            }
            else
            {
                wg.wallGrab = false;
            }
        }
        else if (wg.collidersInLadder > 0 && OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, wg.controller) > 0.9f)
        {
            // new grab instance
            wg.wallGrab = true;
        }
    }

    // returns: true - if successfully completed a wall grab move, false - otherwise
    private bool wallGrabMove(WallGrabber wg)
    {
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, wg.controller) > 0.9f)
        {
            if (!wg.wallGrab && wg.collidersInWall > 0)
            {
                grabStartPos = wg.transform.position;
                grabStartTime = Time.time;
                wg.wallGrab = true;
            }    
        }
        else if (wg.wallGrab)
        {
            Vector3 displacement = grabStartPos - wg.transform.position;
            direction = displacement.normalized;
            speed = displacement.magnitude / (Time.time - grabStartTime);
            wg.wallGrab = false;
            return true;
        }
        return false;
    }

    // similar to UpdateCharacterController() from CharacterControllerDriver
    private void UpdateCapsuleCollider()
    {
        if (rig == null || collider == null)
            return;

        float height = rig.cameraInRigSpaceHeight;
        float skinWidth = 0.08f;

        Vector3 center = rig.cameraInRigSpacePos;
        center.y = height / 2f + skinWidth;

        collider.height = height;
        collider.center = center;
    }
}
