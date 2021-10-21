using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Plug : MonoBehaviour
{
    [HideInInspector] public bool plugged;
    private PhotonView photonView;

    void Start()
    {
        plugged = false;
        photonView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        Transform outlet = collider.transform.parent;
        if (outlet != null && outlet.tag == "Outlet")
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            switch (outlet.name)
            {
                case "WhiteOutlet":
                    plugged = (transform.parent.name == "WhiteWire");
                    photonView.RPC(nameof(RPC_SetPluggedTrue), RpcTarget.OthersBuffered, plugged);
                    break;
                case "YellowOutlet":
                    plugged = (transform.parent.name == "RedWire");
                    photonView.RPC(nameof(RPC_SetPluggedTrue), RpcTarget.OthersBuffered, plugged);
                    break;
                case "BlueOutlet":
                    plugged = (transform.parent.name == "YellowWire");
                    photonView.RPC(nameof(RPC_SetPluggedTrue), RpcTarget.OthersBuffered, plugged);
                    break;
                case "RedOutlet":
                    plugged = (transform.parent.name == "GreenWire");
                    photonView.RPC(nameof(RPC_SetPluggedTrue), RpcTarget.OthersBuffered, plugged);
                    break;
                case "GreenOutlet":
                    plugged = (transform.parent.name == "BlueWire");
                    photonView.RPC(nameof(RPC_SetPluggedTrue), RpcTarget.OthersBuffered, plugged);
                    break;
                default:
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        // OVRGrabbable makes grabbed objects kinematic
        if (GetComponent<Rigidbody>().isKinematic)
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            plugged = false;
        }
    }

    [PunRPC]
    void RPC_SetPluggedTrue(bool setPlugged)
    {
        plugged = setPlugged;
    }
}