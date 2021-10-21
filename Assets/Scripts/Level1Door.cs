using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Level1Door : MonoBehaviour
{
    public int animationLength;     // number of frames to open door
    public float height;            // height of movement
    [HideInInspector] public bool leaksSealed;    // must be true to activate door
    private int frame;              // current frame
    private Vector3 startPos;       // initial door position in local space
    private Vector3 endPos;         // final door position in local space

    private bool correct;
    private GameObject yellowPlug;
    private GameObject bluePlug;
    private GameObject redPlug;
    private GameObject greenPlug;
    private PhotonView photonView;

    void Start()
    {
        frame = 0;
        startPos = transform.localPosition;
        endPos = startPos + new Vector3(0f, height, 0f);
        leaksSealed = false;

        correct = false;
        yellowPlug = GameObject.Find("YellowWire/Plug");
        bluePlug = GameObject.Find("BlueWire/Plug");
        redPlug = GameObject.Find("RedWire/Plug");
        greenPlug = GameObject.Find("GreenWire/Plug");
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!correct &&
            yellowPlug.GetComponent<Plug>().plugged &&
            bluePlug.GetComponent<Plug>().plugged &&
            redPlug.GetComponent<Plug>().plugged &&
            greenPlug.GetComponent<Plug>().plugged)
            correct = true;

        if (leaksSealed && correct && frame <= animationLength)
        {
            photonView.RPC(nameof(RPC_MoveDoor), RpcTarget.AllBuffered, frame);
            frame++;
        }
    }

    [PunRPC]
    void RPC_MoveDoor(int curFrame)
    {
        transform.localPosition = Vector3.Lerp(startPos, endPos, (float)curFrame / animationLength);
    }
}
