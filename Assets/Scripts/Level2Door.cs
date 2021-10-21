using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Door : MonoBehaviour
{
    public int animationLength;     // number of frames to open door
    public float height;            // height of movement
    public bool open;
    private int frame;              // current frame
    private Vector3 startPos;       // initial door position in local space
    private Vector3 endPos;         // final door position in local space

    private void Start()
    {
        frame = 0;
        startPos = transform.localPosition;
        endPos = startPos + new Vector3(0f, height, 0f);
        open = false;        
    }

    private void Update()
    {
        if (open && frame <= animationLength)
        {
            transform.localPosition = Vector3.Lerp(startPos, endPos, (float)frame / animationLength);
            frame++;
        }
    }
}
