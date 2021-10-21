using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class GravityToggle : MonoBehaviour
{
    public Level2Door[] doors;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void StartZeroG()
    {
        foreach(Level2Door door in doors)
        {
            door.open = true;
        }
        photonView.RPC(nameof(RPC_StartZeroG), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_StartZeroG()
    {
        // PLAYER
        GameObject rig = GameObject.Find("OVRCameraRig");

        // disable joystick movement, character controller
        rig.transform.Find("LocomotionSystem").GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
        rig.GetComponent<CharacterController>().enabled = false;
        rig.GetComponent<CharacterControllerDriver>().enabled = false;
        rig.GetComponent<CharacterControllerDriver2>().enabled = false;

        // enable rigid body, capsule collider, 0G movement

        Rigidbody body = rig.AddComponent<Rigidbody>();       
        body.freezeRotation = true;
        rig.GetComponent<PlayerMotion>().InstantiateBody();
        rig.GetComponent<CapsuleCollider>().enabled = true;

        // MAP
        // enable wall rigid bodies
        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            body = wall.AddComponent<Rigidbody>();
            body.isKinematic = true;
        }

        // disable gravity
        Physics.gravity = Vector3.zero;
    }
}
