using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomSelect : MonoBehaviour
{
    public TMP_InputField inputField;

    public void EnterRoom()
    {
        if (!inputField.text.Equals(""))
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            roomOptions.IsVisible = false;
            PhotonNetwork.JoinOrCreateRoom(inputField.text, roomOptions, null);

            GameObject.Find("MenuCameraRig").SetActive(false);
            GameObject.Find("OVRCameraRig").SetActive(true);        }
    }
}
