using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeComplete : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public int whichAbility;

    // 0 - magnet, 1 - laser
    public void UpgradeCompleted ()
    {
        if (whichAbility == 0)
        {
            // find the right hand ReceiveAbility component and set animationDone to true
            rightHand.GetComponent<ReceiveAbility>().animationDone = true;
        } else if (whichAbility == 1)
        {
            // find the left hand ReceiveAbility component and set animationDone to true
            leftHand.GetComponent<ReceiveAbility>().animationDone = true;
        }        
    }
}
