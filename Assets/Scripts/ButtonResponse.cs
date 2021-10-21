using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ButtonResponse : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public NetworkManager networkManager;
    public int whichCharacter;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void ButtonEnter()
    {
        meshRenderer.material.color = Color.blue;
    }

    public void ButtonExit()
    {
        meshRenderer.material.color = Color.white;
    }

    public void ButtonPressed()
    {
        networkManager.OnCharacterSelect(whichCharacter);
        
        photonView.RPC(nameof(RPC_ButtonPressed), RpcTarget.AllBuffered);
    }

    public void EnableButton()
    {
        photonView.RPC(nameof(RPC_EnableButton), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ButtonPressed()
    {
        meshRenderer.material.color = Color.black;
        this.enabled = false;
    }

    [PunRPC]
    void RPC_EnableButton()
    {
        meshRenderer.material.color = Color.white;
        this.enabled = true;
    }
}
