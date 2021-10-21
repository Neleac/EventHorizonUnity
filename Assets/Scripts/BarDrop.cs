using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BarDrop : MonoBehaviour
{
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void DropBar()
    {
        photonView.RPC(nameof(RPC_DropBar), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_DropBar()
    {
        transform.parent = null;
        gameObject.AddComponent<Rigidbody>();
        gameObject.AddComponent<PhotonRigidbodyView>();
    }
}
