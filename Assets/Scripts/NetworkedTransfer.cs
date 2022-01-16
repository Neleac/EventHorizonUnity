using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkedTransfer : MonoBehaviour
{
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView != null && PhotonNetwork.InRoom)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView != null && PhotonNetwork.InRoom)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
}
