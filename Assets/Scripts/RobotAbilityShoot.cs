using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

[RequireComponent(typeof(LineRenderer))]
public class RobotAbilityShoot : MonoBehaviour 
{
    public LineRenderer rayLine;
    public Transform raySource;
    public GameObject model;
    public OVRInput.RawButton activateButton;
    public int reflections;
    public Material newMaterial;
    public Material oldMaterial;
    public MeshRenderer[] generators;
    public GravityToggle gravityToggle;
    public Transform trackingSpace;

    // 0 - magnet power; 1 - laser power
    public int whichAbility;

    public float lineWidth = 0.1f;
    public float lineMaxLength = 1f;

    public bool powerAqcuired;

    private bool holdingObject;
    private int numPowered;
    private PhotonView photonView;
    private bool startZeroG;
    private bool zeroG;
    private GameObject heldObject;

    void Start()
    {
        Vector3[] startLinePositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        rayLine.SetPositions(startLinePositions);
        rayLine.enabled = false;
        powerAqcuired = false;
        model.SetActive(false);
        holdingObject = false;
        numPowered = 0;
        photonView = GetComponent<PhotonView>();
        startZeroG = false;
        zeroG = false;
    }

    void Update()
    {
        bool buttonPressed = OVRInput.Get(activateButton);
        Vector3 endPosition = raySource.position + (lineMaxLength * transform.forward);

        if (powerAqcuired)
        {
            photonView.RPC(nameof(RPC_EnableModel), RpcTarget.AllBuffered, model.transform.position, model.transform.rotation);

            if (buttonPressed)
            {
                rayLine.enabled = true;
                drawLine(endPosition, raySource.position, transform.forward);
            }
            else
            {
                photonView.RPC(nameof(RPC_StopPower), RpcTarget.All);
                stopMagnet();
                photonView.RPC(nameof(RPC_StopLaser), RpcTarget.AllBuffered);
            }
        }
        if (startZeroG && !zeroG)
        {
            zeroG = true;
            gravityToggle.StartZeroG();
        }
    }

    private void drawLine(Vector3 endPosition, Vector3 targetPosition, Vector3 direction)
    {
        if (whichAbility == 0)
        {
            if (holdingObject)
            {
                useMagnet(direction);
                photonView.RPC(nameof(RPC_UseMagnet), RpcTarget.All, targetPosition, heldObject.transform.position);
            } 
            else 
            {
                RaycastHit hit;
                Ray ray = new Ray(targetPosition, direction);
                if (Physics.Raycast(ray, out hit))
                {
                    endPosition = hit.point;

                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject.CompareTag("MagnetObject") && !holdingObject)
                    {
                        heldObject = hitObject;
                        useMagnet(direction);
                    }
                }
                photonView.RPC(nameof(RPC_UseMagnet), RpcTarget.All, targetPosition, endPosition);
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
            }            
        }
        else if (whichAbility == 1)
        {
            photonView.RPC(nameof(RPC_StopLaser), RpcTarget.AllBuffered);
            useLaser();
        }
    }

    private void useMagnet(Vector3 direction)
    {
        Rigidbody heldBody = heldObject.GetComponent<Rigidbody>();
        heldBody.AddForce(trackingSpace.rotation * OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RHand) * heldBody.mass * 20);
        heldBody.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RHand) * -1f;
        heldBody.useGravity = false;
        holdingObject = true;
    }

    private void stopMagnet()
    {
        Rigidbody heldBody = heldObject.GetComponent<Rigidbody>();
        heldBody.useGravity = true;
        heldObject = null;
        holdingObject = false;
    }

    private void useLaser()
    {
        RaycastHit hit;
        Ray ray = new Ray(raySource.position, raySource.forward);

        rayLine.positionCount = 1;
        rayLine.SetPosition(0, raySource.position);
        ArrayList positions = new ArrayList();
        positions.Add(raySource.position);
        float remainingLength = lineMaxLength;

        for (int i = 0; i < reflections; i++)
        {
            if (Physics.Raycast(ray.origin, ray.direction, out hit, remainingLength))
            {
                rayLine.positionCount++;
                rayLine.SetPosition(rayLine.positionCount - 1, hit.point);
                positions.Add(hit.point);
                remainingLength -= Vector3.Distance(ray.origin, hit.point);
                ray = new Ray(hit.point, Vector3.Scale(Vector3.Reflect(ray.direction, hit.normal), new Vector3(1, 0, 1)));
                if (hit.collider.CompareTag("LaserObject"))
                {
                    Transform bar = hit.transform.parent;
                    hit.transform.parent = null;
                    PhotonNetwork.Destroy(hit.collider.gameObject);
                    if (bar.childCount == 0)
                    {
                        bar.GetComponent<BarDrop>().DropBar();                        
                    }
                    break;
                }
                else
                if (!hit.collider.CompareTag("Mirror"))
                {
                    break;
                }
                // change material of generators and increase numPowered
                MeshRenderer[] children = hit.collider.transform.parent.parent.GetComponentsInChildren<MeshRenderer>();
                int[] childIndexes = new int[2];
                int k = 0;
                foreach (MeshRenderer child in children)
                {
                    if (!child.CompareTag("Mirror"))
                    {
                        for(int j = 0; j < generators.Length; j++)
                        {
                            if (generators[j].Equals(child))
                            {
                                childIndexes[k] = j;
                                k++;
                                break;
                            }
                        }
                    }
                }
                photonView.RPC(nameof(RPC_ChangeMaterial), RpcTarget.AllBuffered, childIndexes[0], childIndexes[1]);
            }
            else
            {
                rayLine.positionCount++;
                rayLine.SetPosition(rayLine.positionCount - 1, ray.origin + ray.direction * remainingLength);
                positions.Add(ray.origin + ray.direction * remainingLength);
                break;
            }
        }
        Vector3[] newArray = new Vector3[rayLine.positionCount];
        positions.CopyTo(newArray);
        photonView.RPC(nameof(RPC_UseLaser), RpcTarget.All, newArray, rayLine.positionCount);
    }

    [PunRPC]
    void RPC_StopLaser()
    {
        if (numPowered < 4)
        {
            numPowered = 0;
            foreach (MeshRenderer generator in generators)
            {
                generator.material = oldMaterial;
            }
        } else
        {
            startZeroG = true;
        }
    }

    [PunRPC]
    void RPC_ChangeMaterial(int gen1, int gen2)
    {
        generators[gen1].material = newMaterial;
        generators[gen2].material = newMaterial;
        numPowered++;
    }

    [PunRPC]
    void RPC_EnableModel(Vector3 position, Quaternion rotation)
    {
        model.SetActive(true);
        model.transform.SetPositionAndRotation(position, rotation);
    }

    [PunRPC]
    void RPC_UseMagnet(Vector3 start, Vector3 end)
    {
        rayLine.enabled = true;
        rayLine.SetPosition(0, start);
        rayLine.SetPosition(1, end);
    }

    [PunRPC]
    void RPC_UseLaser(Vector3[] positions, int numPositions)
    {
        rayLine.positionCount = numPositions;
        rayLine.enabled = true;
        for (int i = 0; i < positions.Length; i++)
        {
            rayLine.SetPosition(i, positions[i]);
        }
    }

    [PunRPC]
    void RPC_StopPower()
    {
        rayLine.enabled = false;
    }
}