using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveAbility : MonoBehaviour
{
    public OVRGrabber rightHand;
    public OVRGrabber leftHand;
    public bool animationDone;

    private OVRGrabber thisHand;

    private void Start()
    {
        thisHand = gameObject.GetComponent<OVRGrabber>();
        animationDone = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerPrefs.HasKey("Character"))
        {
            // human powers
            if (PlayerPrefs.GetInt("Character") == 0)
            {
                // ice ability checks
                if (other.gameObject.CompareTag("IceSyringe") &&
                other.GetComponentInParent<OVRGrabbable>().isGrabbed &&
                !GetComponentInParent<FireAbilityShoot>().powerAqcuired &&
                !other.GetComponentInParent<OVRGrabbable>().grabbedBy.Equals(thisHand) &&
                !(rightHand.GetComponentInParent<IceAbilityShoot>().powerAqcuired || leftHand.GetComponentInParent<IceAbilityShoot>().powerAqcuired))
                {
                    other.GetComponentInParent<Animator>().SetBool("TriggerSyringe", true);
                    GetComponentInParent<IceAbilityShoot>().powerAqcuired = true;
                }
                // fire ability checks
                else if (other.gameObject.CompareTag("FireSyringe") &&
                other.GetComponentInParent<OVRGrabbable>().isGrabbed &&
                !GetComponentInParent<IceAbilityShoot>().powerAqcuired &&
                !other.GetComponentInParent<OVRGrabbable>().grabbedBy.Equals(thisHand) &&
                !(rightHand.GetComponentInParent<FireAbilityShoot>().powerAqcuired || leftHand.GetComponentInParent<FireAbilityShoot>().powerAqcuired))
                {
                    other.GetComponentInParent<Animator>().SetBool("TriggerSyringe", true);
                    GetComponentInParent<FireAbilityShoot>().powerAqcuired = true;
                }
            }
            // robot powers
            else if (PlayerPrefs.GetInt("Character") == 1)
            {
                // collided with left hand
                if (thisHand.Equals(leftHand) && other.gameObject.CompareTag("LaserUpgrade"))
                {
                    other.transform.parent.GetComponent<Animator>().enabled = true;
                    other.transform.parent.GetComponent<Animator>().SetBool("StartUpgrade", true);                    
                }
                // collided with right hand
                else if (thisHand.Equals(rightHand) && other.gameObject.CompareTag("MagnetUpgrade"))
                {
                    other.transform.parent.GetComponent<Animator>().enabled = true;
                    other.transform.parent.GetComponent<Animator>().SetBool("StartUpgrade", true);                 
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (PlayerPrefs.HasKey("Character"))
        {
            // while robot's hand is hovering over upgrader, check if animation is done, then give power
            if (PlayerPrefs.GetInt("Character") == 1 &&
                (other.gameObject.CompareTag("MagnetUpgrade") || other.gameObject.CompareTag("LaserUpgrade")) &&
                animationDone)
            {
                if (animationDone)
                {
                    gameObject.GetComponentInParent<RobotAbilityShoot>().powerAqcuired = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // if player moves hand before upgrade complete, stop animation
        if (PlayerPrefs.HasKey("Character"))
        {
            if (PlayerPrefs.GetInt("Character") == 1 && 
                (other.gameObject.CompareTag("MagnetUpgrade") || other.gameObject.CompareTag("LaserUpgrade")))
            {
                other.transform.parent.GetComponent<Animator>().enabled = false;
            }
        }
    }
}
