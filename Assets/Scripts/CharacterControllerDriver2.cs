using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CharacterControllerDriver2 : MonoBehaviour
{
    private XRRig rig;
    private CharacterController controller;
    private CharacterControllerDriver driver;

    void Start()
    {
        rig = GetComponent<XRRig>();
        controller = GetComponent<CharacterController>();
        driver = GetComponent<CharacterControllerDriver>();
    }

    void Update()
    {
        UpdateCharacterController();
    }

    // from CharacterControllerDriver.cs
    protected virtual void UpdateCharacterController()
    {
        if (rig == null || controller == null)
            return;

        var height = Mathf.Clamp(rig.cameraInRigSpaceHeight, driver.minHeight, driver.maxHeight);

        Vector3 center = rig.cameraInRigSpacePos;
        center.y = height / 2f + controller.skinWidth;

        controller.height = height;
        controller.center = center;
    }
}
