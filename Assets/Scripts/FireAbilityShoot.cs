using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FireAbilityShoot : MonoBehaviour
{
    public LineRenderer rayLine;
    public GameObject shootObject;
    public ParticleSystem handParticles;
    public Transform handTip;
    public OVRInput.RawButton activateButton;

    public float lineWidth = 0.1f;
    public float lineMaxLength = 1f;

    public bool powerAqcuired;

    private PhotonView photonView;
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
        shape = shootObject.GetComponent<ParticleSystem>().shape;
    }

    void Update()
    {
        bool buttonPressed = OVRInput.Get(activateButton);
        Vector3 endPosition = handTip.position + (lineMaxLength * transform.forward);

        if (powerAqcuired)
        {
            // show particles on hand to indicate that player has ability
            photonView.RPC(nameof(RPC_FireParticles), RpcTarget.AllBuffered, handParticles.transform.position, handParticles.transform.rotation);

            if (buttonPressed)
            {
                drawLine(endPosition, handTip.position, transform.forward);
            }
            else
            {
                photonView.RPC(nameof(RPC_FireBeamOff), RpcTarget.All);
            }
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
            if (hitObject.CompareTag("FireObject"))
            {
                hitObject.GetComponent<HumanPowerReact>().Melt();
            }
        }

        photonView.RPC(nameof(RPC_FireBeam), RpcTarget.All, targetPosition, endPosition, shootObject.transform.position, shootObject.transform.rotation);
    }

    [PunRPC]
    public void RPC_FireParticles(Vector3 position, Quaternion rotation)
    {
        handParticles.Play();
        handParticles.transform.SetPositionAndRotation(position, rotation);
    }

    [PunRPC]
    public void RPC_FireBeam(Vector3 start, Vector3 end, Vector3 position, Quaternion rotation)
    {
        shootObject.SetActive(true);
        shape.length = Vector3.Distance(end, start);
        shootObject.transform.SetPositionAndRotation(position, rotation);
    }

    [PunRPC]
    public void RPC_FireBeamOff()
    {
        shootObject.SetActive(false);
    }
}