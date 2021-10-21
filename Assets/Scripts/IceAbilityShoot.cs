using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class IceAbilityShoot : MonoBehaviour
{
    public LineRenderer rayLine;
    public GameObject shootObject;
    public ParticleSystem handParticles;
    public Transform handTip;
    public OVRInput.RawButton activateButton;
    public GameObject[] cracks;
    public Level1Door[] doors;

    public float lineWidth = 0.1f;
    public float lineMaxLength = 1f;

    public bool powerAqcuired;

    private PhotonView photonView;
    private int leaksSealed;
    private GameObject[] lights;
    private ParticleSystem.ShapeModule shape;

    void Start()
    {
        Vector3[] startLinePositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        rayLine.SetPositions(startLinePositions);
        rayLine.enabled = false;
        handParticles.Stop();
        shootObject.SetActive(false);
        powerAqcuired = false;
        photonView = GetComponent<PhotonView>();
        leaksSealed = 0;
        lights = GameObject.FindGameObjectsWithTag("HumanRoomLight");
        shape = shootObject.GetComponent<ParticleSystem>().shape;
    }

    void Update()
    {
        bool buttonPressed = OVRInput.Get(activateButton);
        Vector3 endPosition = handTip.position + (lineMaxLength * transform.forward);

        if (powerAqcuired)
        {
            // show particles on hand to indicate that player has ability
            photonView.RPC(nameof(RPC_IceParticles), RpcTarget.AllBuffered, handParticles.transform.position, handParticles.transform.rotation);

            if (buttonPressed)
            {
                /*shootObject.SetActive(true);*/
                rayLine.enabled = true;
                drawLine(endPosition, handTip.position, transform.forward);
            }
            else
            {
                photonView.RPC(nameof(RPC_IceBeamOff), RpcTarget.All);
            }
        }

        // all leaks sealed, restore power in human start room
        if (leaksSealed == 3)
        {
            foreach(Level1Door door in doors)
            {
                door.leaksSealed = true;
            }
            photonView.RPC(nameof(RPC_LightsOn), RpcTarget.AllBuffered);

            leaksSealed = 0;
        }
    }

    private void drawLine(Vector3 endPosition, Vector3 targetPosition, Vector3 direction)
    {
        RaycastHit hit;
        Ray ray = new Ray(targetPosition, direction);

        if (Physics.Raycast(ray, out hit))
        {
            endPosition = hit.point;

            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("IceObject"))
            {
                if (hitObject.GetComponent<HumanPowerReact>().Freeze())
                {
                    for (int i = 0; i < cracks.Length; i++)
                    {
                        if (cracks[i].Equals(hitObject))
                        {
                            photonView.RPC(nameof(RPC_AirLeakSeal), RpcTarget.AllBuffered, i);
                            break;
                        }          
                    }                         
                }
            }
        }

        photonView.RPC(nameof(RPC_IceBeam), RpcTarget.All, targetPosition, endPosition, shootObject.transform.position, shootObject.transform.rotation);
    }

    [PunRPC]
    void RPC_AirLeakSeal(int whichCrack)
    {
        // keeps track of leaks sealed in human start room
        leaksSealed++;
        cracks[whichCrack].transform.parent.gameObject.SetActive(false);
    }

    [PunRPC]
    void RPC_IceParticles(Vector3 position, Quaternion rotation)
    {
        handParticles.Play();
        handParticles.transform.SetPositionAndRotation(position, rotation);
    }

    [PunRPC]
    void RPC_IceBeam(Vector3 start, Vector3 end, Vector3 position, Quaternion rotation)
    {
        shootObject.SetActive(true);
        rayLine.enabled = true;
        rayLine.SetPosition(0, start);
        rayLine.SetPosition(1, end);
        shape.length = Vector3.Distance(end, start);
        shootObject.transform.SetPositionAndRotation(position, rotation);
    }

    [PunRPC]
    void RPC_IceBeamOff()
    {
        rayLine.enabled = false;
        shootObject.SetActive(false);
    }

    [PunRPC]
    void RPC_LightsOn()
    {
        foreach (GameObject light in lights)
        {
            if (light.GetComponent<Light>() != null)
            {
                // point light
                light.GetComponent<Light>().intensity = 5;
            }
            else
            {
                // lamp mesh
                light.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white);
            }
        }
    }
}