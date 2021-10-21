using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPointer : MonoBehaviour
{
    public LineRenderer rayLine;
    public Transform handTip;

    public float lineWidth = 0.1f;
    public float lineMaxLength = 1f;

    private bool inCharacterSelect = true;

    private bool buttonPressed = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);

    private GameObject button;

    void Start()
    {
        Vector3[] startLinePositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        rayLine.SetPositions(startLinePositions);
    }

    void Update()
    {
        buttonPressed = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);

        if (inCharacterSelect)
        {
            RaycastHit hit;
            Ray ray = new Ray(handTip.position, transform.forward);

            Vector3 endPosition = handTip.position + (lineMaxLength * transform.forward);

            if (Physics.Raycast(ray, out hit))
            {
                endPosition = hit.point;

                button = hit.collider.gameObject;
                if (button.GetComponent<ButtonResponse>())
                {
                    button.GetComponent<ButtonResponse>().ButtonEnter();
                } else
                {
                    leftButton();
                }
            } else
            {
                leftButton();
            }

            rayLine.SetPosition(0, handTip.position);
            rayLine.SetPosition(1, endPosition);

            if (buttonPressed)
            {
                if (button.GetComponent<ButtonResponse>().enabled)
                {
                    inCharacterSelect = false;
                    button.GetComponent<ButtonResponse>().ButtonPressed();                    
                }
            }
        } else
        {
            rayLine.enabled = false;
        }
    }

    private void leftButton()
    {
        ButtonResponse[] allButtons = FindObjectsOfType<ButtonResponse>();
        foreach (ButtonResponse button in allButtons)
        {
            button.ButtonExit();
        }
    }
}