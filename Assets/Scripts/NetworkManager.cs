using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject humanPrefab;
    public GameObject robotPrefab;
    public GameObject robotBarrier;

    void Start()
    {
        // connect to server
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // current behavior: join random room, if none available create new room
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    public void OnCharacterSelect(int whichCharacter)
    {
        PlayerPrefs.SetInt("Character", whichCharacter);

        GameObject cameraRig = GameObject.Find("OVRCameraRig");
        if (whichCharacter == 0) 
        {
            // move camera rig to human start position
            cameraRig.transform.position = new Vector3(-72f, 0.1f, -13f);
            cameraRig.transform.rotation = Quaternion.Euler(0, 90, 0);

            PhotonNetwork.Instantiate(this.humanPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0);
        }
        else
        {
            // move camera rig to robot start position
            cameraRig.transform.position = new Vector3(-92f, 0.1f, -30f);
            cameraRig.transform.rotation = Quaternion.Euler(0, -90, 0);

            robotBarrier.SetActive(true);

            PhotonNetwork.Instantiate(this.robotPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0);
        }

        // enable movement
        cameraRig.GetComponent<CharacterController>().enabled = true;
        cameraRig.GetComponent<InputActionManager>().enabled = true;
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (PlayerPrefs.GetInt("Character") == 0)
            GameObject.FindGameObjectWithTag("HumanButton").GetComponent<ButtonResponse>().EnableButton();
        else if (PlayerPrefs.GetInt("Character") == 1)
            GameObject.FindGameObjectWithTag("RobotButton").GetComponent<ButtonResponse>().EnableButton();
    }
}
