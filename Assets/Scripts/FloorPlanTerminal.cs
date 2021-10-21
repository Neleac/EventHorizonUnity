using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FloorPlanTerminal : MonoBehaviour
{
    private GameObject display;
    private GameObject plug;
    private int slide;    

    private const float buttonLimit = 0.75f;
    private Transform leftButton;
    private Transform rightButton;
    private Vector3 leftStart;          // initial local position
    private Vector3 rightStart;         // initial local position
    private bool leftPressed;
    private bool rightPressed;
    private PhotonView photonView;

    void Start()
    {
        display = GameObject.Find("FloorPlanDisplay");
        plug = display.transform.Find("WhiteWire/Plug").gameObject;
        slide = 1;

        leftButton = transform.Find("Base_1/ButtonLeft");
        rightButton = transform.Find("Base_2/ButtonRight");
        leftStart = leftButton.localPosition;
        rightStart = rightButton.localPosition;
        leftPressed = false;
        rightPressed = false;
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        // button motion along positive z axis within limit
        float z = Mathf.Clamp(leftButton.localPosition.z, leftStart.z, leftStart.z + buttonLimit);
        leftButton.localPosition = new Vector3(leftStart.x, leftStart.y, z);
        z = Mathf.Clamp(rightButton.localPosition.z, rightStart.z, rightStart.z + buttonLimit);
        rightButton.localPosition = new Vector3(rightStart.x, rightStart.y, z);
        int prevSlide = slide;

        if (plug.GetComponent<Plug>().plugged)
        {
            GameObject newSlide = display.transform.Find("Slide_" + slide).gameObject;
            newSlide.SetActive(true);

            // left button press
            if (!leftPressed && leftButton.localPosition.z == leftStart.z + buttonLimit)
            {
                leftPressed = true;                
                slide--;
                if (slide == 0)
                    slide = 5;
                photonView.RPC(nameof(RPC_ChangeSlide), RpcTarget.AllBuffered, prevSlide, slide);
            }
            else if (leftPressed && leftButton.localPosition.z == leftStart.z)
            {
                leftPressed = false;
            }

            // right button press
            if (!rightPressed && rightButton.localPosition.z == rightStart.z + buttonLimit)
            {
                rightPressed = true;
                slide++;
                if (slide == 6)
                    slide = 1;
                photonView.RPC(nameof(RPC_ChangeSlide), RpcTarget.AllBuffered, prevSlide, slide);
            }
            else if (rightPressed && rightButton.localPosition.z == rightStart.z)
            {
                rightPressed = false;
            }
        }
        else
        {
            GameObject oldSlide = display.transform.Find("Slide_" + slide).gameObject;
            oldSlide.SetActive(false);
        }
    }

    [PunRPC]
    void RPC_ChangeSlide(int prevSlide, int slide)
    {
        GameObject oldSlide = display.transform.Find("Slide_" + prevSlide).gameObject; 
        oldSlide.SetActive(false);
        GameObject newSlide = display.transform.Find("Slide_" + slide).gameObject;
        newSlide.SetActive(true);
    }
}
