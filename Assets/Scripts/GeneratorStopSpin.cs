using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GeneratorStopSpin : MonoBehaviour
{
    public Transform generator;
    public float speed = 20.0f;

    private bool spinning;
    private bool justStopped;
    private static Quaternion[] vals = { Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 90, 0), 
        Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 270, 0), Quaternion.Euler(0, 360, 0) };
    private Quaternion newRotation = Quaternion.Euler(0, 0, 0);
    private float timeCount = 0f;
    private PhotonView photonView;

    private void Start()
    {
        spinning = false;
        justStopped = false;
        photonView = GetComponent<PhotonView>();
    }

    public void StartSpin ()
    {
        photonView.RPC(nameof(RPC_Spin), RpcTarget.AllBuffered);
    }

    public void StopSpin ()
    {
        photonView.RPC(nameof(RPC_StopSpin), RpcTarget.AllBuffered);
    }

    private void Update()
    {
        if (spinning)
        {
            generator.Rotate(Vector3.up, speed * Time.deltaTime);
        }
        else if (justStopped)
        {
            float minDist = 361f;
            timeCount = 0f;
            foreach (Quaternion val in vals)
            {
                float curDist = Quaternion.Angle(generator.rotation, val);
                if (curDist < minDist)
                {
                    minDist = curDist;
                    newRotation = val;
                }
            }
            generator.rotation = Quaternion.Slerp(generator.rotation, newRotation, timeCount);
            timeCount += Time.deltaTime;
            justStopped = false;
        }
        else if (generator.rotation != newRotation)
        {
            generator.rotation = Quaternion.Slerp(generator.rotation, newRotation, timeCount);
            timeCount += Time.deltaTime / 2;
        }
    }

    [PunRPC]
    void RPC_Spin()
    {
        spinning = true;
    }

    [PunRPC]
    void RPC_StopSpin()
    {
        spinning = false;
        justStopped = true;
    }
}
