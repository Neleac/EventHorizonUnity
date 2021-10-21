using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using System.IO;

[System.Serializable]
public class VRMap
{
    // set in inspector
    public Transform avatarIKTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    // set in code, since OVRCameraRig is in scene hierarchy not character prefab
    private Transform cameraRigTarget;

    public void Map() 
    {
        avatarIKTarget.position = cameraRigTarget.TransformPoint(trackingPositionOffset);
        avatarIKTarget.rotation = cameraRigTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }

    public void Map(Vector3 position, Quaternion rotation) 
    {
        avatarIKTarget.position = position;
        avatarIKTarget.rotation = rotation;
    }

    public void Map(WallGrabber wg) 
    {
        if (!wg.wallGrab)
        {
            Map();
        }
    }
    
    public void setCameraRigTarget(Transform target) {
        cameraRigTarget = target;
    }
}

public class NetworkPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    public VRMap head;
    public VRMap leftHand;
    public VRMap leftThumb;
    public VRMap leftIndex;
    public VRMap leftMiddle;
    public VRMap leftRing;
    public VRMap leftPinky;
    public VRMap rightHand;
    public VRMap rightThumb;
    public VRMap rightIndex;
    public VRMap rightMiddle;
    public VRMap rightRing;
    public VRMap rightPinky;
    
    // handles hand-wall collisions, component on CustomHandLeft/CustomHandRight
    private WallGrabber leftWG;
    private WallGrabber rightWG;

    private Vector3 bodyToHeadDist;
    private PhotonView photonView;
    
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        PhotonNetwork.SerializationRate = 72;

        if (photonView.IsMine)
        {
            // set camera rig targets
            GameObject cameraOffset = GameObject.Find("OVRCameraRig/TrackingSpace");
            head.setCameraRigTarget(cameraOffset.transform.Find("CenterEyeAnchor"));

            Transform leftHandAnchor = cameraOffset.transform.Find("LeftHandAnchor");
            leftHand.setCameraRigTarget(leftHandAnchor);
            leftWG = leftHandAnchor.Find("CustomHandLeft").GetComponent<WallGrabber>();
            Transform leftFingersAnchor = leftHandAnchor.Find("CustomHandLeft/Offset/l_hand_skeletal_lowres/hands:l_hand_world/hands:b_l_hand");
            leftIndex.setCameraRigTarget(leftFingersAnchor.Find("hands:b_l_index1/hands:b_l_index2/hands:b_l_index3"));
            leftMiddle.setCameraRigTarget(leftFingersAnchor.Find("hands:b_l_middle1/hands:b_l_middle2/hands:b_l_middle3"));
            leftRing.setCameraRigTarget(leftFingersAnchor.Find("hands:b_l_ring1/hands:b_l_ring2/hands:b_l_ring3"));
            leftPinky.setCameraRigTarget(leftFingersAnchor.Find("hands:b_l_pinky0/hands:b_l_pinky1/hands:b_l_pinky2/hands:b_l_pinky3"));
            if (gameObject.name == "Astronaut_Pilot_Full(Clone)")
                // human model has 2-bone thumbs
                leftThumb.setCameraRigTarget(leftFingersAnchor.Find("hands:b_l_thumb1/hands:b_l_thumb2/hands:b_l_thumb3"));
            else
                // robot model has 1-bone thumbs
                leftThumb.setCameraRigTarget(leftFingersAnchor.Find("hands:b_l_thumb1/hands:b_l_thumb2"));       

            Transform rightHandAnchor = cameraOffset.transform.Find("RightHandAnchor");
            rightHand.setCameraRigTarget(rightHandAnchor);
            rightWG = rightHandAnchor.Find("CustomHandRight").GetComponent<WallGrabber>();
            Transform rightFingersAnchor = rightHandAnchor.Find("CustomHandRight/Offset/r_hand_skeletal_lowres/hands:r_hand_world/hands:b_r_hand");
            rightIndex.setCameraRigTarget(rightFingersAnchor.Find("hands:b_r_index1/hands:b_r_index2/hands:b_r_index3"));
            rightMiddle.setCameraRigTarget(rightFingersAnchor.Find("hands:b_r_middle1/hands:b_r_middle2/hands:b_r_middle3"));
            rightRing.setCameraRigTarget(rightFingersAnchor.Find("hands:b_r_ring1/hands:b_r_ring2/hands:b_r_ring3"));
            rightPinky.setCameraRigTarget(rightFingersAnchor.Find("hands:b_r_pinky0/hands:b_r_pinky1/hands:b_r_pinky2/hands:b_r_pinky3"));
            if (gameObject.name == "Astronaut_Pilot_Full(Clone)")
                // human model has 2-bone thumbs
                rightThumb.setCameraRigTarget(rightFingersAnchor.Find("hands:b_r_thumb1/hands:b_r_thumb2/hands:b_r_thumb3"));
            else
                // robot model has 1-bone thumbs
                rightThumb.setCameraRigTarget(rightFingersAnchor.Find("hands:b_r_thumb1/hands:b_r_thumb2"));

            bodyToHeadDist = head.avatarIKTarget.position - transform.position;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            // body position and orientation
            transform.position = head.avatarIKTarget.position - bodyToHeadDist;
            transform.forward = Vector3.ProjectOnPlane(head.avatarIKTarget.up, Vector3.up).normalized;

            // body part tracking
            head.Map();

            leftHand.Map(leftWG);
            leftIndex.Map(leftWG);
            leftMiddle.Map(leftWG);
            leftRing.Map(leftWG);
            leftPinky.Map(leftWG);
            if (gameObject.name == "Astronaut_Pilot_Full(Clone)")
                leftThumb.Map(leftWG);

            rightHand.Map(rightWG);
            rightIndex.Map(rightWG);
            rightMiddle.Map(rightWG);
            rightRing.Map(rightWG);
            rightPinky.Map(rightWG);
            if (gameObject.name == "Astronaut_Pilot_Full(Clone)")
                rightThumb.Map(rightWG);
        }
    }

    // prevent robot spazzing thumbs, let OVRCameraRig invisible hands update first
    void LateUpdate() {
        if (gameObject.name == "Robot Kyle(Clone)" && photonView.IsMine)
        {
            leftThumb.Map(leftWG);
            rightThumb.Map(rightWG);
        }
    }

    // sync network player limbs and fingers
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // local player
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

            stream.SendNext(head.avatarIKTarget.position);
            stream.SendNext(head.avatarIKTarget.rotation);

            stream.SendNext(leftHand.avatarIKTarget.position);
            stream.SendNext(leftHand.avatarIKTarget.rotation);
            stream.SendNext(leftIndex.avatarIKTarget.position);
            stream.SendNext(leftIndex.avatarIKTarget.rotation);
            stream.SendNext(leftMiddle.avatarIKTarget.position);
            stream.SendNext(leftMiddle.avatarIKTarget.rotation);
            stream.SendNext(leftRing.avatarIKTarget.position);
            stream.SendNext(leftRing.avatarIKTarget.rotation);
            stream.SendNext(leftPinky.avatarIKTarget.position);
            stream.SendNext(leftPinky.avatarIKTarget.rotation);
            stream.SendNext(leftThumb.avatarIKTarget.position);
            stream.SendNext(leftThumb.avatarIKTarget.rotation);

            stream.SendNext(rightHand.avatarIKTarget.position);
            stream.SendNext(rightHand.avatarIKTarget.rotation);
            stream.SendNext(rightIndex.avatarIKTarget.position);
            stream.SendNext(rightIndex.avatarIKTarget.rotation);
            stream.SendNext(rightMiddle.avatarIKTarget.position);
            stream.SendNext(rightMiddle.avatarIKTarget.rotation);
            stream.SendNext(rightRing.avatarIKTarget.position);
            stream.SendNext(rightRing.avatarIKTarget.rotation);
            stream.SendNext(rightPinky.avatarIKTarget.position);
            stream.SendNext(rightPinky.avatarIKTarget.rotation);
            stream.SendNext(rightThumb.avatarIKTarget.position);
            stream.SendNext(rightThumb.avatarIKTarget.rotation);
        }
        else
        {
            // network player
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();

            head.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());

            leftHand.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            leftIndex.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            leftMiddle.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            leftRing.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            leftPinky.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            leftThumb.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());

            rightHand.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            rightIndex.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            rightMiddle.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            rightRing.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            rightPinky.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            rightThumb.Map((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
        }
    }
}
