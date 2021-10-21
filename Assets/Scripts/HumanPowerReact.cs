using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HumanPowerReact : MonoBehaviour
{
    // "IceBlock", "Water", "AirLeak"
    public string type;
    public GameObject robotBarrier;

    private PhotonView photonView;
    private Animator animator;
    private Animator parentAnimator;
    private ParticleSystem.EmissionModule emission;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        emission = GetComponent<ParticleSystem>().emission;
    }

    // returns: true - air leak sealed, false - otherwise
    public bool Freeze()
    {
        switch (type)
        {
            case "AirLeak":                
                if (emission.rateOverTime.constant > 0)
                {
                    emission.rateOverTime = emission.rateOverTime.constant - 2;
                }
                else
                {                    
                    return true;
                }
                break;
            case "Water":
                GetComponent<Animator>().SetBool("Freezing", true);
                GetComponent<Animator>().SetBool("Melting", false);
                photonView.RPC(nameof(RPC_WaterFreeze), RpcTarget.AllBuffered);
                break;
            case "Generator":
                GetComponentInParent<Animator>().SetBool("Melt", false);
                GetComponentInParent<Animator>().SetBool("Freeze", true);
                break;
            default:
                // if no type or if type is ice block, do nothing when being frozen
                break;
        }
        return false;
    }

    public void Melt()
    {
        switch (type)
        {
            case "Water":
                GetComponent<Animator>().SetBool("Melting", true);
                GetComponent<Animator>().SetBool("Freezing", false);
                photonView.RPC(nameof(RPC_WaterMelt), RpcTarget.AllBuffered);
                break;
            case "Generator":
                GetComponentInParent<Animator>().SetBool("Freeze", false);
                GetComponentInParent<Animator>().SetBool("Melt", true);
                break;
            default:
                // if no type or if type is air leak, do nothing when being melted
                break;
        }
    }

    [PunRPC]
    void RPC_WaterFreeze()
    {        
        Vector3 targetPos = new Vector3(transform.position.x, -10f, transform.position.z);
        if (robotBarrier.activeSelf)
        {
            robotBarrier.transform.position = targetPos;
        }
        gameObject.tag = "FireObject";
    }

    [PunRPC]
    void RPC_WaterMelt()
    {        
        Vector3 targetPos = new Vector3(transform.position.x, 1.59f, transform.position.z);
        if (robotBarrier.activeSelf)
        {
            robotBarrier.transform.position = targetPos;
        }
        gameObject.tag = "IceObject";
    }
}