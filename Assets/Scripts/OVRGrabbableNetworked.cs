using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OVRGrabbableNetworked : OVRGrabbable
{
    private PhotonView photonView;

    protected override void Start()
    {
        photonView = GetComponent<PhotonView>();
        base.Start();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        if (photonView != null)
        {
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);            
            base.GrabBegin(hand, grabPoint);
        }        
    }
}
